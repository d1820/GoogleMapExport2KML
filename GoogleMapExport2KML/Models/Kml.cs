using System.Xml.Serialization;

namespace GoogleMapExport2KML.Models;

[XmlRoot("kml", Namespace = "https://schemas.opengis.net/kml/2.3")]
public class Kml
{
    public Document Document { get; set; } = new Document();
}
