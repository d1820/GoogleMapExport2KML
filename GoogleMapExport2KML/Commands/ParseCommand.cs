using System.Diagnostics;
using GoogleMapExport2KML.Extensions;
using GoogleMapExport2KML.Models;
using GoogleMapExport2KML.Processors;
using GoogleMapExport2KML.Services;
using Microsoft.Playwright;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GoogleMapExport2KML.Commands;

public class ParseCommand : AsyncCommand<ParseSettings>
{
    private readonly CsvProcessor _csvProcessor;
    private readonly DataLocationProcessor _datalocationProcessor;
    private readonly GeolocationProcessor _geolocationProcessor;
    private readonly KMLService _kmlService;
    private readonly StatExecutor _statExecutor;
    private readonly VerboseRenderer _verboseRenderer;

    public ParseCommand(CsvProcessor csvProcessor, KMLService kmlService,
        GeolocationProcessor geolocationProcessor,
        DataLocationProcessor datalocationProcessor,
        StatExecutor statExecutor,
        VerboseRenderer verboseRenderer)
    {
        _csvProcessor = csvProcessor;
        _kmlService = kmlService;
        _geolocationProcessor = geolocationProcessor;
        _datalocationProcessor = datalocationProcessor;
        _statExecutor = statExecutor;
        _verboseRenderer = verboseRenderer;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ParseSettings settings)
    {
        var kml = new Kml();
        var fi = new FileInfo(settings.OutputFile);
        if (!fi.Exists)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            fi = new FileInfo(Path.Combine(currentDirectory, settings.OutputFile));
        }
        kml.Document.Name = fi.Name.Replace(fi.Extension, "");

        var sw = new Stopwatch();
        var eventSW = new Stopwatch();
        if (!settings.DryRun)
        {
            sw.Start();
        }
        else
        {
            var panel = new Panel("               DRY RUN               ");
            panel.Border = BoxBorder.Ascii;
            panel.Padding = new Padding(0, 1, 0, 1);
            AnsiConsole.Write(panel);
        }

        if(settings.MaxDegreeOfParallelism > 5)
        {
            AnsiConsole.MarkupLine($"[red bold]!!WARNING: Setting --parallel higher then 10 will cause excessive memory usage!!{Environment.NewLine}[/]");
        }
        List<CsvLineItemError> csvErrors = [];
        List<CsvLineItemResponse> csvResponses = [];
        await _statExecutor.ExecuteAsync("CSV Parsing", async () =>
        {
            var results = await _csvProcessor.ProcessAsync(settings.Files, settings);
            csvErrors.AddRange(results.Where(w => w.HasErrors).SelectMany(s => s.Errors).ToList());
            csvResponses.AddRange(results);
        });
        if (csvErrors.Any())
        {
            if (settings.StopOnError)
            {
                DisplayErrorTable(csvErrors, "CSV Parsing Errors");
                return -1;
            }
            else
            {
                await File.WriteAllLinesAsync(Path.Combine(fi.Directory!.FullName, "error.log"), csvErrors.Select(s => s.ToString()));
            }
        }

        _verboseRenderer.Render(settings, () => AnsiConsole.MarkupLine($"[yellow bold]Processed {csvResponses.Count} files.[/]"));

        var errorCode = await _statExecutor.ExecuteAsync("Processing Latitude Longitude Data", async () =>
        {
            var geoLocations = csvResponses.Where(w => !w.HasErrors)
                                            .SelectMany(s => s.Results)
                                            .Where(w => w.URL.Contains("/search/")).ToList();

            if (settings.DryRun)
            {
                AnsiConsole.MarkupLine($"[darkorange3 bold]{_geolocationProcessor.EstimateRunTime(geoLocations)}[/]");
            }
            else
            {
                var geoResponse = await _geolocationProcessor.ProcessAsync(geoLocations, settings);

                if (!geoResponse.IsSuccess)
                {
                    if (settings.StopOnError)
                    {
                        DisplayErrorTable(geoResponse.Errors, "Placemark Parsing Errors");
                        return -1;
                    }
                    else
                    {
                        await File.WriteAllLinesAsync(Path.Combine(fi.Directory!.FullName, "error.log"), geoResponse.Errors.Select(s => s.ToString()));
                    }
                }
                _verboseRenderer.Render(settings, () => AnsiConsole.MarkupLine($"[yellow bold]Processed {geoResponse.Placemarks.Count} geolocations.[/]"));
                kml.Document.Placemarks.AddRange(geoResponse.Placemarks);
            }
            return 0;
        });
        if (errorCode != 0)
        {
            return errorCode;
        }

        errorCode = await _statExecutor.ExecuteAsync("Processing Google Place Lookups", async () =>
        {
            var dataPlaces = csvResponses.Where(w => !w.HasErrors)
                                       .SelectMany(s => s.Results)
                                       .Where(w => w.URL.Contains("/place/")).ToList();

            if (settings.DryRun)
            {
                AnsiConsole.MarkupLine($"[darkorange3 bold]{_datalocationProcessor.EstimateRunTime(dataPlaces, settings)}[/]");
            }
            else
            {
                var dataResponse = await _datalocationProcessor.ProcessAsync(dataPlaces, settings);
                if (settings.StopOnError && !dataResponse.IsSuccess)
                {
                    if (settings.StopOnError)
                    {
                        DisplayErrorTable(dataResponse.Errors, "Placemark Parsing Errors");
                        return -1;
                    }
                    else
                    {
                        await File.WriteAllLinesAsync(Path.Combine(fi.Directory!.FullName, "error.log"), dataResponse.Errors.Select(s => s.ToString()));
                    }
                }
                _verboseRenderer.Render(settings, () => AnsiConsole.MarkupLine($"[yellow bold]Processed {dataResponse.Placemarks.Count} places.[/]"));
                kml.Document.Placemarks.AddRange(dataResponse.Placemarks);
            }
            return 0;
        });
        if (errorCode != 0)
        {
            return errorCode;
        }

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine($"File(s) would be written to [yellow]{fi.Directory!.FullName}[/]");
        }
        else
        {
            await _statExecutor.ExecuteAsync("Writing KML Output", () =>
            {
                EnsureParentDirectories(fi.FullName, settings);
                _kmlService.CreateKML(kml, fi, settings);
                return Task.CompletedTask;
            });

            sw.Stop();
            AnsiConsole.WriteLine("");
            AnsiConsole.MarkupLine($"[green bold]KML file(s) successfully generated. Placements: {kml.Document.Placemarks.Count}.[/]");
            AnsiConsole.WriteLine("");
            if (settings.IncludeStats)
            {
                foreach (var stat in _statExecutor.GetResults())
                {
                    AnsiConsole.MarkupLine($"{stat.Event}: {stat.TotalMs.MsToTime()}");
                }
            }
            AnsiConsole.MarkupLine($"Total Processing Time: {sw.ElapsedMilliseconds.MsToTime()}");
            AnsiConsole.MarkupLine($"File(s) written to [yellow]{fi.Directory!.FullName}[/]");
        }

        return 0;
    }

    private static void DisplayErrorTable(List<CsvLineItemError> csvErrors, string title)
    {
        AnsiConsole.MarkupLine($"{title}: [bold red]{csvErrors.Count}[/]");
        //output error rows in table
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.LeftAligned();
        // Add some columns
        table.AddColumn("RowIndex").Centered();
        table.AddColumn("ColumnIndex").Centered();
        table.AddColumn("Error");
        table.AddColumn("Row");

        foreach (var line in csvErrors)
        {
            table.AddRow(line.RowIndex.ToString(), line.ColumnIndex.ToString(), line.Error ?? "Unknown", line.Row ?? "No Row Data");
        }

        AnsiConsole.Write(table);
    }

    private static void EnsureParentDirectories(string filePath, ParseSettings settings)
    {
        // Get the directory path without the file name
        string directoryPath = Path.GetDirectoryName(filePath);

        // Create all necessary parent directories
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            if (settings.Trace)
            {
                AnsiConsole.WriteLine($"Parent directories created: {directoryPath}");
            }
        }
    }
}
