using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleMapExport2KML.Mappings;
using GoogleMapExport2KML.Models;
using OpenQA.Selenium;
using Spectre.Console;
using static GoogleMapExport2KML.Commands.ParseCommand;

namespace GoogleMapExport2KML.Processors;
public class GeolocationProcessor
{
    private readonly Mapper _mapper;

    public GeolocationProcessor(Mapper mapper)
    {
        _mapper = mapper;
    }
    public async Task<ProcessorResponse> ProcessAsync(List<CsvLineItem> geoLocations, ParseSettings settings, StatusContext ctx)
    {
        var response = new ProcessorResponse();
        if (geoLocations.Count > 0)
        {
            // Update the status and spinner
            ctx.Status($"Parsing Geolocations 1 of {geoLocations.Count}");
            ctx.Spinner(Spinner.Known.Circle);
            ctx.SpinnerStyle(Style.Parse("blue"));
            ctx.Refresh();

            //process all the ones that already have lat and long
            for (var i = 0; i < geoLocations.Count; i++)
            {
                var line = geoLocations[i];
                ctx.Status($"Parsing Geolocations {i + 1} of {geoLocations.Count}");
                ctx.Refresh();
                await Task.Delay(200);
                if (settings.LogLevel == LogLevel.Debug)
                {
                    AnsiConsole.MarkupLine($"Processing {line.URL}");
                }
                var pm = _mapper.MapToPlacement(line, settings.IncludeCommentInDescription);
                if (pm.HasError)
                {
                    response.Errors.Add(new CsvLineItemError
                    {
                        RowIndex = line.RowNumber,
                        Error = pm.ErrorMessage
                    });
                    continue;
                }
                response.Placemarks.Add(pm.Placemark!);
            }
        }
        return response;
    }
}
