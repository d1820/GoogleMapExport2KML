using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using GoogleMapExport2KML.Extensions;
using GoogleMapExport2KML.Factories;
using GoogleMapExport2KML.Mappings;
using GoogleMapExport2KML.Models;
using Spectre.Console;
using static GoogleMapExport2KML.Commands.ParseCommand;

namespace GoogleMapExport2KML.Processors;

public class DataLocationProcessor
{
    private readonly ChromeDriverPool _chromeFactory;
    private readonly Mapper _mapper;
    private int _delay = 3000;
    private double _spinupFactor = 0.7; //amount to factor in for driver creation when estimating

    public DataLocationProcessor(Mapper mapper, ChromeDriverPool chromeFactory)
    {
        _mapper = mapper;
        _chromeFactory = chromeFactory;
    }

    public string EstimateRunTime(List<CsvLineItem> dataPlaces, ParseSettings settings)
    {
        return $"Parsing {dataPlaces.Count} Google data locations. Est Time: {((dataPlaces.Count * (_delay * 2.5)) / (settings.MaxDegreeOfParallelism * _spinupFactor)).MsToTime()}";
    }

    public async Task<ProcessorResponse> ProcessAsync(List<CsvLineItem> dataPlaces, ParseSettings settings)
    {
        var response = new ProcessorResponse();
        if (dataPlaces.Count > 0)
        {
            var _queue = new ConcurrentBag<int>();
            var msgFormat = $"Parsing {{0}} of {dataPlaces.Count} Google data locations. Est Time: {((dataPlaces.Count * (_delay * 2.5)) / (settings.MaxDegreeOfParallelism * _spinupFactor)).MsToTime()}";
            return await AnsiConsole.Status()
           .Spinner(Spinner.Known.Circle)
           .SpinnerStyle(Style.Parse("blue bold"))
           .StartAsync(string.Format(msgFormat, 1), async ctx =>
           {
               ctx.Refresh();
               var timeout = TimeSpan.FromSeconds(settings.QueryPlacesTimeoutSeconds);

               var actionBlock = new ActionBlock<CsvLineItem>(async line =>
               {
                   using (var kmlDriver = _chromeFactory.Get())
                   {
                       _queue.Add(1);
                       ctx.Status(string.Format(msgFormat, _queue.Count));
                       ctx.Refresh();
                       if (settings.Verbose)
                       {
                           AnsiConsole.MarkupLine($"Processing {line.DisplayName}");
                       }

                       kmlDriver.Instance.Navigate().GoToUrl(line.URL);

                       var startTime = DateTime.Now;
                       while (DateTime.Now - startTime < timeout)
                       {
                           await Task.Delay(_delay);
                           if (kmlDriver.Instance.Url.Contains('@'))
                           {
                               break;
                           }
                       }

                       line.URL = kmlDriver.Instance.Url;
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
                       }
                       else
                       {
                           response.Placemarks.Add(pm.Placemark!);
                       }
                   }
               },
               new ExecutionDataflowBlockOptions
               {
                   MaxDegreeOfParallelism = settings.MaxDegreeOfParallelism
               });

               foreach (var item in dataPlaces)
               {
                   actionBlock.Post(item);
               }
               actionBlock.Complete();

               // Wait for all messages to propagate through the network.
               await actionBlock.Completion;
               return response;
           });
        }
        return response;
    }
}
