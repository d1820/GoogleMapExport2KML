using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using GoogleMapExport2KML.Commands;
using GoogleMapExport2KML.Extensions;
using GoogleMapExport2KML.Interfaces;
using GoogleMapExport2KML.Mappings;
using GoogleMapExport2KML.Models;
using GoogleMapExport2KML.Services;
using Microsoft.VisualBasic;
using OpenQA.Selenium;
using Spectre.Console;

namespace GoogleMapExport2KML.Processors;

public class DataLocationProcessor
{
    private readonly IWebDriverPool _webFactory;
    private readonly StatExecutor _statExecutor;
    private readonly Mapper _mapper;
    private int _delay = 3000;
    private double _spinupFactor = 0.7; //amount to factor in for driver creation when estimating

    public DataLocationProcessor(Mapper mapper, IWebDriverPool webFactory, StatExecutor statExecutor)
    {
        _mapper = mapper;
        _webFactory = webFactory;
        _statExecutor = statExecutor;
    }

    public string EstimateRunTime(List<CsvLineItem> dataPlaces, ParseSettings settings)
    {
        return $"Parsing {dataPlaces.Count} Google data locations. Est Time: {(dataPlaces.Count * (_delay * 2.5) / (settings.MaxDegreeOfParallelism * _spinupFactor)).MsToTime()}";
    }

    public async Task<ProcessorResponse> ProcessAsync(List<CsvLineItem> dataPlaces, ParseSettings settings)
    {
        var response = new ProcessorResponse();
        var cts = new CancellationTokenSource();
        if (dataPlaces.Count > 0)
        {
            var _queue = new ConcurrentBag<int>();
            var msgFormat = $"Parsing {{0}} of {dataPlaces.Count} Google data locations. Est Time: {(dataPlaces.Count * (_delay * 2.5) / (settings.MaxDegreeOfParallelism * _spinupFactor)).MsToTime()}";
            return await AnsiConsole.Status()
           .Spinner(Spinner.Known.Circle)
           .SpinnerStyle(Style.Parse("blue bold"))
           .StartAsync(string.Format(msgFormat, 1), async ctx =>
           {
               try
               {
                   ctx.Refresh();
                   var timeout = TimeSpan.FromSeconds(settings.QueryPlacesTimeoutSeconds);
                   var chunkSize = (int)Math.Ceiling((double)dataPlaces.Count / settings.BatchCount);
                   var batches = dataPlaces.Chunk(settings.BatchCount).ToArray();
                   //var test = dataPlaces.Chunk(settings.BatchCount).ToArray();
                   AnsiConsole.MarkupLine($"Created {chunkSize} batches of {settings.BatchCount} for processing");
                   var batchIdx = 1;
                   foreach (var batch in batches)
                   {
                       await _statExecutor.ExecuteAsync($"Processing Latitude Longitude Data Batch {batchIdx}", async () =>
                       {
                           var actionBlock = CreateActionBLock(settings, ctx, response, cts, _queue, msgFormat, timeout);
                           await _webFactory.InitializeAsync(settings);
                           foreach (var item in batch)
                           {
                               actionBlock.Post(item);
                           }
                           actionBlock.Complete();
                           await actionBlock.Completion;

                           await _webFactory.ShutDownDriversAsync();
                       });
                       batchIdx++;
                   }
                   //actionBlock.Complete();
                   //// Wait for all messages to propagate through the network.
                   //await actionBlock.Completion;
               }
               catch (TaskCanceledException)
               {
                   //NOOP
               }
               finally
               {
                   await _webFactory.ShutDownDriversAsync();
               }
               return response;
           });
        }
        return response;
    }

    private ActionBlock<CsvLineItem> CreateActionBLock(ParseSettings settings, StatusContext ctx, ProcessorResponse response, CancellationTokenSource cts, ConcurrentBag<int> _queue, string msgFormat, TimeSpan timeout)
    {
        var actionBlock = new ActionBlock<CsvLineItem>(async line =>
        {
            var retryCount = 0;

            using (var driver = _webFactory.Get())
            {
                try
                {
                    await ProcessLineAsync(driver.Browser, settings, ctx, line, response, _queue, msgFormat, timeout, "", cts);
                }
                catch (Exception ex)
                {
                    if (retryCount > 2)
                    {
                        if (settings.Trace)
                        {
                            AnsiConsole.MarkupLine($"[grey50]Skipping Record. RetryCount: {retryCount}. Line: {line}. Error: {ex.Message}[/]");
                        }
                        response.Errors.Add(new CsvLineItemError
                        {
                            RowIndex = line.RowNumber,
                            ColumnIndex = 0,
                            Row = line.DisplayName,
                            Error = $"Selenium Exception occurred. Unable to process row. Error {ex.Message}"
                        });
                        if (settings.StopOnError)
                        {
                            cts.Cancel();
                        }
                        return;
                    }
                    await ProcessLineAsync(driver.Browser, settings, ctx, line, response, _queue, msgFormat, timeout, "Retry ", cts);
                    retryCount++;
                }
            }
        },
        new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = settings.MaxDegreeOfParallelism,
            CancellationToken = cts.Token
        });
        return actionBlock;
    }

    private async Task ProcessLineAsync(IBrowser browser, ParseSettings settings, StatusContext ctx, CsvLineItem line, ProcessorResponse response, ConcurrentBag<int> _queue, string msgFormat, TimeSpan timeout, string prefix = "", CancellationTokenSource cts = null)
    {
        _queue.Add(1);
        ctx.Status(string.Format(msgFormat, _queue.Count));
        ctx.Refresh();
        if (settings.Trace)
        {
            AnsiConsole.MarkupLine($"{prefix}Processing {line.DisplayName}");
            AnsiConsole.MarkupLine($"[grey50]Using Instance {browser.Id} for URL {line.URL}[/]");
        }
        try
        {
            await browser.GotoUrlAsync(line.URL);
        }
        catch (WebDriverTimeoutException wde) when (wde.Message.Contains("Timed out receiving message from renderer", StringComparison.InvariantCultureIgnoreCase))
        {
            //try one more time
            await browser.GotoUrlAsync(line.URL);
        }

        var startTime = DateTime.Now;
        while (DateTime.Now - startTime < timeout)
        {
            await Task.Delay(_delay, cts.Token);
            if (browser.GetUrl().Contains('@'))
            {
                break;
            }
        }

        line.URL = browser.GetUrl();
        var pm = _mapper.MapToPlacement(line, settings.IncludeCommentInDescription);
        if (pm.HasError)
        {
            response.Errors.Add(new CsvLineItemError
            {
                RowIndex = line.RowNumber,
                ColumnIndex = 0,
                Row = line.DisplayName,
                Error = pm.ErrorMessage
            });
            if (settings.StopOnError)
            {
                cts.Cancel();
            }
        }
        else
        {
            response.Placemarks.Add(pm.Placemark!);
        }
        //await Task.Delay(1000);
    }
}
