using GoogleMapExport2KML.Models;

namespace GoogleMapExport2KML.Mappings;

public class Mapper
{
    public PlacemarkResult MapToPlacement(CsvLineItem csv, bool includeComment)
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
            desc += " " + csv.Comment ?? string.Empty;
        }

        var pointResult = MapToPoint(csv.URL);
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

    public ParsePointResult MapToPoint(string url)
    {
        ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));

        //https://www.google.com/maps/search/33.895005,-112.333546
        //OR
        //https://www.google.com/maps/place/North+Fork+Campground/@38.6117469,-106.3202388,17z/data=!3m1!4b1!4m6!3m5!1s0x871544e7cd62c109:0x1b22906b6daddb80!8m2!3d38.6117469!4d-106.3202388!16s%2Fg%2F1tg8k5fc?entry=ttu
        if (url.Contains("https://www.google.com/maps/search/"))
        {
            var coords = url.Replace("https://www.google.com/maps/search/", "");
            var splitResult = SplitCoords(url, coords);
            if (!string.IsNullOrEmpty(splitResult.error))
            {
                return new ParsePointResult { ErrorMessage = splitResult.error };
            }
            return new ParsePointResult { Point = new Point($"{splitResult.longitude},{splitResult.latitude}") }; //this order is specific to the spec
        }

        if (url.Contains("https://www.google.com/maps/place/"))
        {
            //coords: @38.6117469,-106.3202388,17z
            try
            {
                var coords = url.Replace("https://www.google.com/maps/place/", "").Split("/").Take(2).Last();
                var splitResult = SplitCoords(url, coords);
                if (!string.IsNullOrEmpty(splitResult.error))
                {
                    return new ParsePointResult { ErrorMessage = splitResult.error };
                }
                return new ParsePointResult { Point = new Point($"{splitResult.longitude},{splitResult.latitude}") }; //this order is specific to the spec
            }
            catch (Exception ex)
            {
                return new ParsePointResult { ErrorMessage = $"{ex.Message}. Url: {url}" };
            }
        }
        return new ParsePointResult { ErrorMessage = $"Url does not match any existing parser. Skipping Point Parsing. Url: {url}" };
    }

    private static (string? latitude, string? longitude, string? error) SplitCoords(string url, string coords)
    {
        var parts = coords.Replace("@", "").Split(",").Take(2).ToList();
        if (parts == null || parts.Count == 0)
        {
            return (null, null, $"Url does not match any existing parser. Skipping Point Parsing. Url: {url}");
        }
        return (parts[0]?.Trim(), parts[1]?.Trim(), null);
    }
}
