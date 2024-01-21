using System.Xml.Serialization;
using CsvHelper.Configuration;
using GoogleMapExport2KML.Extensions;

namespace GoogleMapExport2KML.Models;
public class CsvLineItem
{
    public string Title { get; set; }
    public string Note { get; set; }
    public string URL { get; set; }
    public string Comment { get; set; }
    public int RowNumber { get; set; }
}

public class CsvLineItemError
{
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }
    public string Row { get; set; }
    public string Error { get; set; }
}

public class CsvLineItemResponse
{

    public List<CsvLineItem> Results { get; set; } = new List<CsvLineItem>();
    public List<CsvLineItemError> Errors { get; set; } = new List<CsvLineItemError>();
    public bool HasErrors => Errors?.Any() == true;
}

public sealed class CsvLineItemMap : ClassMap<CsvLineItem>
{
    public CsvLineItemMap()
    {
        Map(m => m.Title);
        Map(m => m.Note);
        Map(m => m.URL);
        Map(m => m.Comment);
        Map(m => m.RowNumber).Convert(row => row.Row.Parser.RawRow);
    }
}

/*
 <kml xmlns#"http://www.opengis.net/kml/2.50">
  <Placemark>
    <name>{{NAME}}</name>
    <description>{{DESC}}</description>
    <Point>
      <coordinates>{{COORD}}</coordinates>
    </Point>
  </Placemark>
</kml>
 */

[XmlRoot("kml", Namespace = "http://www.opengis.net/kml/2.50")]
public class Kml
{
    [XmlArray("Placemarks")]
    [XmlArrayItem("Placemark")]
    public List<Placemark> Placemarks { get; set; } = new List<Placemark>();
}

public class Placemark
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Point Point { get; set; }
}

public class PlacemarkResult
{
    public string ErrorMessage { get; set; }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public Placemark? Placemark { get; set; }
}

public class ParsePointResult
{
    public string ErrorMessage { get; set; }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public Point? Point { get; set; }
}
public class Point
{
    public string Coordinates { get; set; }

    public Point(string coordinates)
    {
        Coordinates = coordinates;
    }

    public static ParsePointResult ParseFromUrl(string url)
    {
        ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));

        //https://www.google.com/maps/search/33.895005,-112.333546
        //OR
        //https://www.google.com/maps/place/Hawley+Lake+Campground+Area/data=!4m2!3m1!1s0x8728a32cc0f42b29:0x9401773f7a022a97
        if (url.Contains("https://www.google.com/maps/search/"))
        {
            var coords = url.Replace("https://www.google.com/maps/search/", "");
            //parse coords
            return new ParsePointResult { Point = new Point(coords) };
        }

        if (url.Contains("https://www.google.com/maps/place/"))
        {
            //0x8728a32cc0f42b29:0x9401773f7a022a97
            var data = url.Split("!1s").Last();
            var coords = data.Split(":");
            var latitude = coords[0].GetLatitude();
            var longitude = coords[1].GetLongitude();
            return new ParsePointResult { Point = new Point($"{latitude},{longitude}") };
        }
        return new ParsePointResult { ErrorMessage = $"Url does not match any existing parser. Skipping Point Parsing. Url: {url}" };
    }
}
