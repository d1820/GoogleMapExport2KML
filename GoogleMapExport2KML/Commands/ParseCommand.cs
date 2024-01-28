using System;
using System.ComponentModel;
using System.Diagnostics;
using GoogleMapExport2KML.Extensions;
using GoogleMapExport2KML.Models;
using GoogleMapExport2KML.Processors;
using GoogleMapExport2KML.Services;
using OpenQA.Selenium;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GoogleMapExport2KML.Commands;

public class ParseCommand : AsyncCommand<ParseCommand.ParseSettings>
{
    private readonly CsvProcessor _csvProcessor;
    private readonly DataLocationProcessor _datalocationProcessor;
    private readonly GeolocationProcessor _geolocationProcessor;
    private readonly KMLService _kmlService;

    public ParseCommand(CsvProcessor csvProcessor, KMLService kmlService,
        GeolocationProcessor geolocationProcessor,
        DataLocationProcessor datalocationProcessor)
    {
        _csvProcessor = csvProcessor;
        _kmlService = kmlService;
        _geolocationProcessor = geolocationProcessor;
        _datalocationProcessor = datalocationProcessor;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ParseSettings settings)
    {
        var sw = new Stopwatch();
        if (!settings.DryRun)
        {
            sw.Start();
        }
        var results = await _csvProcessor.ProcessAsync(settings.Files, settings);
        var csvErrors = results.Where(w => w.HasErrors).SelectMany(s => s.Errors).ToList();
        if (settings.StopOnError && csvErrors.Any())
        {
            DisplayErrorTable(csvErrors, "CSV Parsing Errors");
            return -1;
        }

        if (settings.LogLevel == LogLevel.Debug)
        {
            AnsiConsole.MarkupLine($"[yellow bold]---------------------------------[/]");
        }
        AnsiConsole.MarkupLine($"[yellow bold]Processed {results.Length} files.[/]");
        if (settings.LogLevel == LogLevel.Debug)
        {
            Console.WriteLine("");
        }
        var kml = new Kml();
        var fi = new FileInfo(settings.OutputFileName);
        kml.Document.Name = fi.Name.Replace(fi.Extension, "");
        var geoLocations = results.Where(w => !w.HasErrors)
                                        .SelectMany(s => s.Results)
                                        .Where(w => w.URL.Contains("/search/")).ToList();

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine($"[darkorange3 bold]{_geolocationProcessor.EstimateRunTime(geoLocations)}[/]");
        }
        else
        {
            var geoResponse = await _geolocationProcessor.ProcessAsync(geoLocations, settings);

            if (settings.StopOnError && !geoResponse.IsSuccess)
            {
                DisplayErrorTable(geoResponse.Errors, "Placemark Parsing Errors");
                return -1;
            }
            if (settings.LogLevel == LogLevel.Debug)
            {
                AnsiConsole.MarkupLine($"[yellow bold]------------------------------------------------------[/]");
            }
            AnsiConsole.MarkupLine($"[yellow bold]Processed {geoResponse.Placemarks.Count} geolocations.[/]");
            if (settings.LogLevel == LogLevel.Debug)
            {
                Console.WriteLine("");
            }
            kml.Document.Placemarks.AddRange(geoResponse.Placemarks);
        }

        var dataPlaces = results.Where(w => !w.HasErrors)
                                       .SelectMany(s => s.Results)
                                       .Where(w => w.URL.Contains("/place/")).ToList();

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine($"[darkorange3 bold]{_datalocationProcessor.EstimateRunTime(dataPlaces)}[/]");
        }
        else
        {
            var dataResponse = await _datalocationProcessor.ProcessAsync(dataPlaces, settings);
            if (settings.StopOnError && !dataResponse.IsSuccess)
            {
                DisplayErrorTable(dataResponse.Errors, "Placemark Parsing Errors");
                return -1;
            }
            if (settings.LogLevel == LogLevel.Debug)
            {
                AnsiConsole.MarkupLine($"[yellow bold]------------------------------------------------[/]");
            }
            AnsiConsole.MarkupLine($"[yellow bold]Processed {dataResponse.Placemarks.Count} places.[/]");
            if (settings.LogLevel == LogLevel.Debug)
            {
                Console.WriteLine("");
            }
            kml.Document.Placemarks.AddRange(dataResponse.Placemarks);
        }

        var outFilePath = settings.OutputFileName;
        if (!File.Exists(settings.OutputFileName))
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            outFilePath = Path.Combine(currentDirectory, settings.OutputFileName);
        }
        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine($"File would be written to [yellow]{outFilePath}[/]");
        }
        else
        {
            EnsureParentDirectories(outFilePath);
            _kmlService.CreateKML(kml, outFilePath);

            sw.Stop();
            Console.WriteLine("");
            AnsiConsole.MarkupLine($"[green bold]KML file successfully generated. Placements: {kml.Document.Placemarks.Count}.[/]");
            Console.WriteLine("");
            AnsiConsole.MarkupLine($"Total Processing Time: {sw.ElapsedMilliseconds.MsToTime()}");
            AnsiConsole.MarkupLine($"File written to [yellow]{outFilePath}[/]");
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

    private static void EnsureParentDirectories(string filePath)
    {
        // Get the directory path without the file name
        string directoryPath = Path.GetDirectoryName(filePath);

        // Create all necessary parent directories
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Console.WriteLine($"Parent directories created: {directoryPath}");
        }
    }

    public class ParseSettings : KmlBaseSettings
    {
        [CommandOption("-f|--file <VALUES>")]
        [Description("The csv files to parse")]
        public string[] Files { get; set; }

        [CommandOption("--includeComments")]
        [Description("If true. Adds any comment from the csv column to the description")]
        public bool IncludeCommentInDescription { get; set; }

        [CommandOption("-l|--loglevel")]
        [Description("The log level of the output. Default: Info")]
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        [CommandOption("-o|--output")]
        [Description("The name of the output KML file")]
        public string OutputFileName { get; set; }

        [CommandOption("-t|--timeout")]
        [Description("The timeout to wait on each lookup for coordinates from Google. Default 10s")]
        public double QueryPlacesTimeoutSeconds { get; set; } = 10;

        [CommandOption("--stopOnError")]
        [Description("If true. Stops parsing on any csv row error.")]
        public bool StopOnError { get; set; }

        [CommandOption("--dryrun")]
        [Description("If true. Runs through the files and estimates times to completion.")]
        public bool DryRun { get; set; }

        public override ValidationResult Validate()
        {
            var filesValid = Files?.All(File.Exists);
            var nameValid = !string.IsNullOrEmpty(OutputFileName);
            if (filesValid != true)
            {
                return ValidationResult.Error("One or more files can not be found");
            }
            if (!nameValid)
            {
                return ValidationResult.Error("output file is required");
            }

            return ValidationResult.Success();
        }
    }
}
