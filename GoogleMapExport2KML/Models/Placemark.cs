using System.Xml.Serialization;

namespace GoogleMapExport2KML.Models;

public class Placemark
{
    [XmlElement(ElementName = "description")]
    public string Description { get; set; }

    [XmlElement(ElementName = "name")]
    public string Name { get; set; }

    public Point Point { get; set; }
}
