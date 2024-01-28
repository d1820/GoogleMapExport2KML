using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using GoogleMapExport2KML.Models;
using OpenQA.Selenium;
using Spectre.Console;
using static GoogleMapExport2KML.Commands.ParseCommand;

namespace GoogleMapExport2KML.Processors;

public class CsvProcessor
{
    public async Task<CsvLineItemResponse[]> ProcessAsync(string[] files, ParseSettings settings)
    {
        return await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("green bold"))
            .StartAsync($"Parsing CSV files.", async ctx =>
            {

                var tasks = new List<Task<CsvLineItemResponse>>();
                foreach (var file in files)
                {
                    var fi = new FileInfo(file);
                    if (settings.LogLevel == LogLevel.Debug)
                    {
                        AnsiConsole.MarkupLine($"Parsing file {fi.Name}");
                    }
                    ctx.Refresh();
                    tasks.Add(ExecuteAsync(file));
                }

               return await Task.WhenAll(tasks);
            });
    }

    private async Task<CsvLineItemResponse> ExecuteAsync(string fileName)
    {
        var errors = new List<CsvLineItemError>();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            LineBreakInQuotedFieldIsBadData = false,
            BadDataFound = null,
            ReadingExceptionOccurred = (args) =>
            {
                var ex = args.Exception;
                errors.Add(new CsvLineItemError
                {
                    RowIndex = ex.Context.Parser.RawRow,
                    Row = ex.Context.Parser.RawRecord,
                    Error = ex.Message,
                    ColumnIndex = ex.Context.Reader.CurrentIndex
                });
                return true;
            },
            MissingFieldFound = (args) =>
            {
                Console.WriteLine(args.ToString());
            },
        };
        var response = new CsvLineItemResponse();

        using (var reader = new StreamReader(fileName))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<CsvLineItemMap>();
            var lines = csv.GetRecordsAsync<CsvLineItem>();
            await foreach (var line in lines)
            {
                response.Results.Add(line);
            }
        }
        response.Errors = errors;

        return response;
    }
}
