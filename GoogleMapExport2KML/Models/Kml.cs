using System.Xml.Serialization;

namespace GoogleMapExport2KML.Models;

[XmlRoot("kml", Namespace = "http://www.opengis.net/kml/2.50")]
public class Kml
{
    public Document Document { get; set; } = new Document();
}
