using System.Xml.Serialization;
using GoogleMapExport2KML.Commands;
using GoogleMapExport2KML.Models;
using Spectre.Console;

namespace GoogleMapExport2KML.Services;

public class KMLService
{
    public XmlSerializerNamespaces GetNamespace()
    {
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("xsd", "https://schemas.opengis.net/kml/2.3/ogckml23.xsd");
        return namespaces;
    }

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
            serializer.Serialize(writer, kml, GetNamespace());
            return;
        }

        SplitIntoFiles(kml, outputFile.Directory!, settings.PlacementsPerFile, of);
    }

    public Kml? ParseKmlFile(string kmlFile)
    {
        var serializer = new XmlSerializer(typeof(Kml));
        using TextReader reader = new StreamReader($"{kmlFile}");
        return serializer.Deserialize(reader) as Kml;
    }

    public string SplitIntoFiles(Kml kml, DirectoryInfo outputFile, int placementsPerFile, string outputFilePath, bool trace = false)
    {
        var chunks = kml.Document.Placemarks.Chunk(placementsPerFile).ToList();
        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[1];
            var fileName = $"{kml.Document.Name}{i + 1}.kml";
            outputFilePath = Path.Combine(outputFile.FullName, fileName);
            var newKml = new Kml();
            newKml.Document.Name = kml.Document.Name;
            newKml.Document.Placemarks = chunk.ToList();
            var serializer = new XmlSerializer(typeof(Kml));
            using TextWriter writer = new StreamWriter($"{outputFilePath}");
            serializer.Serialize(writer, newKml);
            if (trace)
            {
                AnsiConsole.MarkupLine($"Created file {outputFilePath}");
            }
        }
        return outputFilePath;
    }
}
