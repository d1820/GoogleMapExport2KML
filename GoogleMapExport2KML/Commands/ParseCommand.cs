using System.ComponentModel;
using GoogleMapExport2KML.Models;
using GoogleMapExport2KML.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using TinyCsvParser.Mapping;

namespace GoogleMapExport2KML.Commands;

public class ParseCommand : Command<ParseCommand.Settings>
{
    private readonly CsvParser _csvParser;
    private readonly KMLService _kmlService;

    public ParseCommand(CsvParser csvParser, KMLService kmlService)
    {
        _csvParser = csvParser;
        _kmlService = kmlService;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("green bold"))
            .Start("Parsing CSV files", ctx =>
            {
                var allItems = new List<CsvMappingResult<CsvLineItem>>();
                foreach (var file in settings.Files)
                {
                    var fi = new FileInfo(file);
                    AnsiConsole.MarkupLine($"Parsing file {fi.Name}");
                    allItems.AddRange(_csvParser.Parse(file));
                }

                // Update the status and spinner
                ctx.Status("Building KML output");
                ctx.Spinner(Spinner.Known.Circle);
                ctx.SpinnerStyle(Style.Parse("blue"));
                var kml = new Kml();

                var csvErrors = allItems.Where(w => !w.IsValid).ToList();
                if (settings.StopOnError && csvErrors.Any())
                {
                    DisplayErrorTable(csvErrors, "CSV Parsing Errors");
                    return;
                }

                foreach (var line in allItems.Where(w => w.IsValid))
                {
                    var pm = Map(line.Result, settings.IncludeCommentInDescription);
                    if (pm.HasError)
                    {
                        csvErrors.Add(new CsvMappingResult<CsvLineItem>
                        {
                            RowIndex = line.RowIndex,
                            Error = new CsvMappingError { ColumnIndex = 3, Value = pm.ErrorMessage }
                        });
                        continue;
                    }
                    kml.Placemarks.Add(pm.Placemark!);
                }

                if (settings.StopOnError && csvErrors.Any())
                {
                    DisplayErrorTable(csvErrors, "Placemark Parsing Errors");
                    return;
                }

                ctx.Status("Generating KML output");
                ctx.Spinner(Spinner.Known.Circle);
                ctx.SpinnerStyle(Style.Parse("blue"));
                var outFilePath = settings.OutputFileName;
                if (!File.Exists(settings.OutputFileName))
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    outFilePath = Path.Combine(currentDirectory, settings.OutputFileName);
                }

                _kmlService.CreateKML(kml, outFilePath);
                AnsiConsole.MarkupLine($"[green bold] KML file successfully generated. Placements: {allItems.Count}.[/] [grey]Saved to {outFilePath}[/]");
            });
    }

    private static void DisplayErrorTable(List<CsvMappingResult<CsvLineItem>> csvErrors, string title)
    {
        AnsiConsole.MarkupLine($"{title}: [bold red]{csvErrors.Count}[/]");
        //output error rows in table
        var table = new Table();
        table.Border(TableBorder.Rounded);
        // Add some columns
        table.AddColumn("Row").Centered();
        table.AddColumn("Column").Centered();
        table.AddColumn("Error");

        foreach (var line in csvErrors)
        {
            table.AddRow(line.RowIndex.ToString(), line.Error.ColumnIndex.ToString(), line.Error.Value);
        }
        // Render the table to the console
        AnsiConsole.Write(table);
    }

    public PlacemarkResult Map(CsvLineItem csv, bool includeComment)
    {
        var desc = csv.Note;
        if (includeComment)
        {
            if (!desc.EndsWith("."))
            {
                desc += "." + (csv.Comment ?? string.Empty);
            }
        }
        var pointResult = Point.ParseFromUrl(csv.URL);
        if (pointResult.HasError)
        {
            return new PlacemarkResult { ErrorMessage = pointResult.ErrorMessage };
        }
        var pm = new Placemark
        {
            Name = csv.Title,
            Description = desc,
            Point = pointResult.Point!
        };
        return new PlacemarkResult
        {
            Placemark = pm
        };
    }



    public class Settings : CommandSettings
    {
        [CommandOption("-f|--file <VALUES>")]
        [Description("The csv files to parse")]
        public string[] Files { get; set; }

        [CommandOption("-o|--output")]
        [Description("The name of the output KML file")]
        public string OutputFileName { get; set; }

        [CommandOption("--includeComments")]
        [Description("If true. Adds any comment from the csv column to the description")]
        public bool IncludeCommentInDescription { get; set; } = false;

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
