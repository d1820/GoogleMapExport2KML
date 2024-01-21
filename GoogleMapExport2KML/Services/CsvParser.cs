using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using GoogleMapExport2KML.Mappings;
using GoogleMapExport2KML.Models;
using TinyCsvParser;
using TinyCsvParser.Mapping;

namespace GoogleMapExport2KML.Services;
public class CsvParser
{
    private CsvParser<CsvLineItem> _csvParser;

    public CsvParser()
    {
        CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
        var csvMapper = new CsvLineItemMapping();
        _csvParser = new CsvParser<CsvLineItem>(csvParserOptions, csvMapper);


    }

    public ReadOnlyCollection<CsvMappingResult<CsvLineItem>> Parse(string filename)
    {
        return _csvParser.ReadFromFile(filename, System.Text.Encoding.ASCII).ToList().AsReadOnly();
    }
}
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
