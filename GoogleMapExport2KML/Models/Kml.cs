using System.Xml.Serialization;

namespace GoogleMapExport2KML.Models;

[XmlRoot("kml", Namespace = "http://www.opengis.net/kml/2.50")]
public class Kml
{
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

    [XmlArray("Placemarks")]
    [XmlArrayItem("Placemark")]
    public List<Placemark> Placemarks { get; set; } = new List<Placemark>();
}
