using System.Xml.Serialization;

namespace GoogleMapExport2KML.Models;

public class Point
{
    [XmlElement(ElementName = "coordinates")]
    public string Coordinates { get; set; }

    public Point()
    {
    }

    public Point(string coordinates)
    {
        Coordinates = coordinates;
    }
}
