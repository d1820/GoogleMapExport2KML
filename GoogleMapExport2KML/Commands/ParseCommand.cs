using System.ComponentModel;
using GoogleMapExport2KML.Models;
using GoogleMapExport2KML.Services;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Spectre.Console;
using Spectre.Console.Cli;
using System;

namespace GoogleMapExport2KML.Commands;

public class ParseCommand : AsyncCommand<ParseCommand.Settings>
{
    private readonly CsvParser _csvParser;
    private readonly KMLService _kmlService;

    public ParseCommand(CsvParser csvParser, KMLService kmlService)
    {
        _csvParser = csvParser;
        _kmlService = kmlService;
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

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var status = 0;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("green bold"))
            .StartAsync("Parsing CSV files", async ctx =>
            {
                ctx.Refresh();
                var allItems = new List<CsvLineItemResponse>();
                var tasks = new List<Task<CsvLineItemResponse>>();
                foreach (var file in settings.Files)
                {
                    var fi = new FileInfo(file);
                    AnsiConsole.MarkupLine($"Parsing file {fi.Name}");
                    ctx.Refresh();
                    tasks.Add(_csvParser.ParseAsync(file));
                }

                var results = await Task.WhenAll(tasks);

                var csvErrors = results.Where(w => w.HasErrors).SelectMany(s => s.Errors).ToList();

                if (settings.StopOnError && csvErrors.Any())
                {
                    DisplayErrorTable(csvErrors, "CSV Parsing Errors");
                    status = -1;
                    return;
                }

                // Update the status and spinner
                ctx.Status("Parsing Geolocations");
                ctx.Spinner(Spinner.Known.Circle);
                ctx.SpinnerStyle(Style.Parse("blue"));
                ctx.Refresh();
                var kml = new Kml();

                //process all the ones that already have lat and long
                foreach (var line in results.Where(w => !w.HasErrors)
                                                .SelectMany(s => s.Results)
                                                .Where(w => w.URL.Contains("/search/")))
                {
                    // Set ChromeOptions to run the browser in headless mode
                    //ChromeOptions options = new ChromeOptions();
                    //options.AddArgument("--headless");

                    //using (IWebDriver driver = new ChromeDriver( options))
                    //{
                    //    // Navigate to the desired URL
                    //    driver.Navigate().GoToUrl(line.URL);

                    //    // Wait for 3 seconds
                    //    await Task.Delay(3000);

                    //    // Read the HTML content
                    //    string htmlContent = driver.PageSource;

                    //    // Use the HTML content as needed
                    //    Console.WriteLine(driver.Url);
                    //}

                    var pm = Map(line, settings.IncludeCommentInDescription);
                    if (pm.HasError)
                    {
                        csvErrors.Add(new CsvLineItemError
                        {
                            RowIndex = line.RowNumber,
                            Error = pm.ErrorMessage
                        });
                        continue;
                    }
                    kml.Placemarks.Add(pm.Placemark!);
                }

                if (settings.StopOnError && csvErrors.Any())
                {
                    DisplayErrorTable(csvErrors, "Placemark Parsing Errors");
                    status = -1;
                    return;
                }

                ctx.Status("Parsing Google data locations");
                ctx.Spinner(Spinner.Known.Circle);
                ctx.SpinnerStyle(Style.Parse("blue"));
                ctx.Refresh();
                //Set ChromeOptions to run the browser in headless mode
                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--headless");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--headless");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--disable-crash-reporter");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--disable-in-process-stack-traces");
                options.AddArgument("--disable-logging");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--log-level=3");
                options.AddArgument("--output=/dev/null");
                options.SetLoggingPreference(LogType.Driver, LogLevel.Off);
                options.SetLoggingPreference(LogType.Browser, LogLevel.Off);
                options.SetLoggingPreference(LogType.Client, LogLevel.Off);
                options.AddAdditionalOption

                using (IWebDriver driver = new ChromeDriver(options))
                {
                    foreach (var line in results.Where(w => !w.HasErrors)
                                                .SelectMany(s => s.Results)
                                                .Where(w => w.URL.Contains("/place/")))
                    {

                        // Navigate to the desired URL
                        driver.Navigate().GoToUrl(line.URL);

                        // Wait for 3 seconds
                        await Task.Delay(3000);

                        // Use the HTML content as needed
                        Console.WriteLine("Found: " + driver.Url);

                        line.URL = driver.Url;

                        var pm = Map(line, settings.IncludeCommentInDescription);
                        if (pm.HasError)
                        {
                            csvErrors.Add(new CsvLineItemError
                            {
                                RowIndex = line.RowNumber,
                                Error = pm.ErrorMessage
                            });
                            continue;
                        }
                        kml.Placemarks.Add(pm.Placemark!);
                    }
                }

                if (settings.StopOnError && csvErrors.Any())
                {
                    DisplayErrorTable(csvErrors, "Placemark Parsing Errors");
                    status = -1;
                    return;
                }

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

                _kmlService.CreateKML(kml, outFilePath);
                AnsiConsole.MarkupLine($"[green bold] KML file successfully generated. Placements: {allItems.Count}.[/] [grey]Saved to {outFilePath}[/]");
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

    public PlacemarkResult Map(CsvLineItem csv, bool includeComment)
    {
        var desc = csv.Note;
        if (csv.URL.Contains("/place/"))
        {
            var name = csv.URL.Replace("https://www.google.com/maps/place/", "").Split("/").First().Replace("+", " ");
            if (!string.IsNullOrEmpty(name))
            {
                desc = name + ". " + csv.Note;
            }
        }

        if (includeComment)
        {
            if (!desc.EndsWith("."))
            {
                desc += ".";
            }
            desc += (" " + csv.Comment ?? string.Empty);
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
}
