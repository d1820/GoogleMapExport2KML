using System.ComponentModel;
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
    private readonly DatalocationProcessor _datalocationProcessor;
    private readonly GeolocationProcessor _geolocationProcessor;
    private readonly KMLService _kmlService;

    public ParseCommand(CsvProcessor csvProcessor, KMLService kmlService,
        GeolocationProcessor geolocationProcessor,
        DatalocationProcessor datalocationProcessor)
    {
        _csvProcessor = csvProcessor;
        _kmlService = kmlService;
        _geolocationProcessor = geolocationProcessor;
        _datalocationProcessor = datalocationProcessor;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ParseSettings settings)
    {
        var status = 0;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("green bold"))
            .StartAsync("Parsing CSV files", async ctx =>
            {
                ctx.Refresh();
                var results = await _csvProcessor.ProcessAsync(settings.Files, ctx);
                var csvErrors = results.Where(w => w.HasErrors).SelectMany(s => s.Errors).ToList();
                if (settings.StopOnError && csvErrors.Any())
                {
                    DisplayErrorTable(csvErrors, "CSV Parsing Errors");
                    status = -1;
                    return;
                }

                var kml = new Kml();
                var geoLocations = results.Where(w => !w.HasErrors)
                                                .SelectMany(s => s.Results)
                                                .Where(w => w.URL.Contains("/search/")).ToList();

                var geoResponse = await _geolocationProcessor.ProcessAsync(geoLocations, settings, ctx);

                if (settings.StopOnError && !geoResponse.IsSuccess)
                {
                    DisplayErrorTable(geoResponse.Errors, "Placemark Parsing Errors");
                    status = -1;
                    return;
                }
                kml.Placemarks.AddRange(geoResponse.Placemarks);

                var dataPlaces = results.Where(w => !w.HasErrors)
                                               .SelectMany(s => s.Results)
                                               .Where(w => w.URL.Contains("/place/")).ToList();

                var dataResponse = await _datalocationProcessor.ProcessAsync(dataPlaces, settings, ctx);
                if (settings.StopOnError && !dataResponse.IsSuccess)
                {
                    DisplayErrorTable(dataResponse.Errors, "Placemark Parsing Errors");
                    status = -1;
                    return;
                }
                kml.Placemarks.AddRange(dataResponse.Placemarks);

                ctx.Status("Generating KML output");
                ctx.Spinner(Spinner.Known.Circle);
                ctx.SpinnerStyle(Style.Parse("blue"));
                ctx.Refresh();
                var outFilePath = settings.OutputFileName;
                if (!File.Exists(settings.OutputFileName))
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    outFilePath = Path.Combine(currentDirectory, settings.OutputFileName);
                }
                EnsureParentDirectories(outFilePath);
                _kmlService.CreateKML(kml, outFilePath);
                var path = new TextPath(outFilePath).RootColor(Color.Grey)
                                .SeparatorColor(Color.Grey)
                                .StemColor(Color.Blue)
                                .LeafColor(Color.Grey);
                AnsiConsole.MarkupLine($"[green bold] KML file successfully generated. Placements: {kml.Placemarks.Count}.[/]");
                AnsiConsole.Write(path);
            });
        return status;
    }

    private static void DisplayErrorTable(List<CsvLineItemError> csvErrors, string title)
    {
        AnsiConsole.MarkupLine($"{title}: [bold red]{csvErrors.Count}[/]");
        //output error rows in table
        var table = new Table();
        table.Border(TableBorder.Rounded);
        // Add some columns
        table.AddColumn("RowIndex").Centered();
        table.AddColumn("ColumnIndex").Centered();
        table.AddColumn("Error");
        table.AddColumn("Row");

        foreach (var line in csvErrors)
        {
            table.AddRow(line.RowIndex.ToString(), line.ColumnIndex.ToString(), line.Error, line.Row);
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

    public class ParseSettings : CommandSettings
    {
        [CommandOption("-f|--file <VALUES>")]
        [Description("The csv files to parse")]
        public string[] Files { get; set; }

        [CommandOption("--includeComments")]
        [Description("If true. Adds any comment from the csv column to the description")]
        public bool IncludeCommentInDescription { get; set; } = false;

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
        public bool StopOnError { get; set; } = false;

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
