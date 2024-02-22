using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using GoogleMapExport2KML.Commands;
using GoogleMapExport2KML.Models;
using Spectre.Console;

namespace GoogleMapExport2KML.Processors;

public class CsvProcessor
{
    public async Task<CsvLineItemResponse[]> ProcessAsync(string[] files, ParseSettings settings)
    {
        return await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("green bold"))
            .StartAsync("Parsing CSV files.", async ctx =>
            {
                var tasks = new List<Task<CsvLineItemResponse>>();
                foreach (var file in files)
                {
                    var fi = new FileInfo(file);
                    if (settings.Verbose || settings.Trace)
                    {
                        AnsiConsole.MarkupLine($"Parsing file {fi.Name}");
                    }
                    ctx.Refresh();
                    tasks.Add(ExecuteAsync(file, settings));
                }

                return await Task.WhenAll(tasks);
            });
    }

    private async Task<CsvLineItemResponse> ExecuteAsync(string fileName, ParseSettings settings)
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
                AnsiConsole.MarkupLine(args.ToString() ?? "");
            },
        };
        var response = new CsvLineItemResponse();

        using (var reader = new StreamReader(fileName, new FileStreamOptions { Access = FileAccess.Read }))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<CsvLineItemMap>();
            var lines = csv.GetRecordsAsync<CsvLineItem>();
            await foreach (var line in lines)
            {
                line.URL = line.URL.Replace("@", "at");
                if (settings.Trace)
                {
                    AnsiConsole.MarkupLine($"    Imported line: {line}");
                }
                response.Results.Add(line);
            }
        }
        response.Errors = errors;

        return response;
    }
}
