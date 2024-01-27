using GoogleMapExport2KML.Factories;
using GoogleMapExport2KML.Mappings;
using GoogleMapExport2KML.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Spectre.Console;
using static GoogleMapExport2KML.Commands.ParseCommand;

namespace GoogleMapExport2KML.Processors;

public class DatalocationProcessor
{
    private readonly ChromeFactory _chromeFactory;
    private readonly Mapper _mapper;

    public DatalocationProcessor(Mapper mapper, ChromeFactory chromeFactory)
    {
        _mapper = mapper;
        _chromeFactory = chromeFactory;
    }

    public async Task<ProcessorResponse> ProcessAsync(List<CsvLineItem> dataPlaces, ParseSettings settings, StatusContext ctx)
    {
        var response = new ProcessorResponse();
        if (dataPlaces.Count > 0)
        {
            ctx.Status($"Parsing Google data locations 1 of {dataPlaces.Count}");
            ctx.Spinner(Spinner.Known.Circle);
            ctx.SpinnerStyle(Style.Parse("blue"));
            ctx.Refresh();
            var timeout = TimeSpan.FromSeconds(settings.QueryPlacesTimeoutSeconds);
            using (ChromeDriver driver = _chromeFactory.CreateDriver())
            {
                for (var i = 0; i < dataPlaces.Count; i++)
                {
                    var line = dataPlaces[i];
                    // Use the HTML content as needed
                    ctx.Status($"Parsing Google data locations {i + 1} of {dataPlaces.Count}");
                    ctx.Refresh();
                    if (settings.LogLevel == LogLevel.Debug)
                    {
                        AnsiConsole.MarkupLine($"Processing {line.URL}");
                    }

                    // Navigate to the desired URL
                    driver.Navigate().GoToUrl(line.URL);

                    var startTime = DateTime.Now;
                    while (DateTime.Now - startTime < timeout)
                    {
                        await Task.Delay(3000);
                        if (driver.Url.Contains('@'))
                        {
                            break;
                        }
                    }

                    line.URL = driver.Url;

                    var pm = _mapper.MapToPlacement(line, settings.IncludeCommentInDescription);
                    if (pm.HasError)
                    {
                        response.Errors.Add(new CsvLineItemError
                        {
                            RowIndex = line.RowNumber,
                            Error = pm.ErrorMessage
                        });
                        if (settings.StopOnError)
                        {
                            break;
                        }
                        continue;
                    }
                    response.Placemarks.Add(pm.Placemark!);
                }
            }
        }
        return response;
    }
}
