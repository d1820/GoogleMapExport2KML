using System.Xml.Serialization;
using GoogleMapExport2KML.Commands;
using GoogleMapExport2KML.Models;

namespace GoogleMapExport2KML.Services;

public class KMLService
{
    public void CreateKML(Kml kml, FileInfo outputFile, ParseSettings settings)
    {
        var of = outputFile.FullName;
        if (!outputFile.Extension.Equals(".kml"))
        {
            of += ".kml";
        }
        if (settings.PlacementsPerFile == -1)
        {
            // Create XmlSerializer
            var serializer = new XmlSerializer(typeof(Kml));
            using TextWriter writer = new StreamWriter($"{of}");
            serializer.Serialize(writer, kml);
            return;
        }

        var chunks = kml.Document.Placemarks.Chunk(settings.PlacementsPerFile).ToList();
        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[1];
            var fileName = $"{kml.Document.Name}{i + 1}.kml";
            of = Path.Combine(outputFile.Directory!.FullName, fileName);
            var newKml = new Kml();
            newKml.Document.Name = kml.Document.Name;
            newKml.Document.Placemarks = chunk.ToList();
            var serializer = new XmlSerializer(typeof(Kml));
            using TextWriter writer = new StreamWriter($"{of}");
            serializer.Serialize(writer, newKml);
        }
    }
}
