using GoogleMapExport2KML.Extensions;
using GoogleMapExport2KML.Mappings;
using GoogleMapExport2KML.Models;
using Spectre.Console;
using static GoogleMapExport2KML.Commands.ParseCommand;

namespace GoogleMapExport2KML.Processors;

public class GeolocationProcessor
{
    private readonly int _delay = 200;
    private readonly Mapper _mapper;

    public GeolocationProcessor(Mapper mapper)
    {
        _mapper = mapper;
    }

    public string EstimateRunTime(List<CsvLineItem> geoLocations)
    {
        return $"Parsing {geoLocations.Count} Google geolocations. Est Time: {(geoLocations.Count * _delay).MsToTime()}";
    }

    public async Task<ProcessorResponse> ProcessAsync(List<CsvLineItem> geoLocations, ParseSettings settings)
    {
        var response = new ProcessorResponse();
        if (geoLocations.Count > 0)
        {
            var msgFormat = $"Parsing {{0}} of {geoLocations.Count} Google geolocations. Est Time: {(geoLocations.Count * _delay).MsToTime()}";
            return await AnsiConsole.Status()
           .Spinner(Spinner.Known.Circle)
           .SpinnerStyle(Style.Parse("blue bold"))
           .StartAsync(string.Format(msgFormat, 1), async ctx =>
           {
               ctx.Refresh();
               //process all the ones that already have lat and long
               for (var i = 0; i < geoLocations.Count; i++)
               {
                   var line = geoLocations[i];
                   ctx.Status(string.Format(msgFormat, i + 1));
                   ctx.Refresh();
                   await Task.Delay(_delay);
                   if (settings.Verbose)
                   {
                       AnsiConsole.MarkupLine($"Processing {line.DisplayName}");
                   }
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
                       continue;
                   }
                   response.Placemarks.Add(pm.Placemark!);
               }
               return response;
           });
        }
        return response;
    }
}
