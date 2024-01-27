using System.Xml.Serialization;
using GoogleMapExport2KML.Models;

namespace GoogleMapExport2KML.Services;

public class KMLService
{
    public void CreateKML(Kml kml, string outputFile)
    {
        if (!outputFile.EndsWith(".kml"))
        {
            outputFile += ".kml";
        }
        // Create XmlSerializer
        var serializer = new XmlSerializer(typeof(Kml));
        using TextWriter writer = new StreamWriter($"{outputFile}");
        serializer.Serialize(writer, kml);
    }
}
