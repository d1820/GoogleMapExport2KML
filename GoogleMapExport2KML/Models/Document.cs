using System.Xml.Serialization;

namespace GoogleMapExport2KML.Models;

public class Document
{
    [XmlElement(ElementName = "name")]
    public string Name { get; set; }

    [XmlElement("Placemark")]
    public List<Placemark> Placemarks { get; set; } = new List<Placemark>();
}
