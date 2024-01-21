using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CsvHelper;
using CsvHelper.Configuration;
using GoogleMapExport2KML.Models;


namespace GoogleMapExport2KML.Services;
public class CsvParser
{
    private Regex _lineEndingRegex;
    private Regex _newlineRegex;

    public CsvParser()
    {
        _lineEndingRegex = new Regex($",\\n", RegexOptions.Multiline);
        _newlineRegex = new Regex($"\\n");
    }

    public async Task<CsvLineItemResponse> ParseAsync(string fileName)
    {
        //var content = File.ReadAllText(fileName);
        //var lines = _lineEndingRegex.Split(content).ToList();

        //for (var i = 0; i < lines.Count; i++)
        //{
        //    if (i == 0)
        //    {
        //        //get rid of header
        //        lines[i] = lines[i].Replace("Title,Note,URL,Comment\n", "");
        //    }
        //    lines[i] = _newlineRegex.Replace(lines[i], " ");
        //}

        //var cleanedContent = string.Join(Environment.NewLine, lines);
        //clean it
        var errors = new List<CsvLineItemError>();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            LineBreakInQuotedFieldIsBadData = false,
            BadDataFound = null,
            ReadingExceptionOccurred = (ReadingExceptionOccurredArgs args) =>
            {
                var ex = args.Exception;
                errors.Add(new CsvLineItemError
                {
                    RowIndex = ex.Context.Parser.RawRow,
                    Row = ex.Context.Parser.RawRecord,
                    Error = ex.Message,
                    ColumnIndex = ex.Context.Reader.CurrentIndex
                });
                return true;
            },
            MissingFieldFound = (MissingFieldFoundArgs args) =>
            {
                Console.WriteLine(args.ToString());
            },
        };
        var response = new CsvLineItemResponse();

        using (var reader = new StreamReader(fileName))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<CsvLineItemMap>();
            response.Results = csv.GetRecordsAsync<CsvLineItem>();
        }
        response.Errors = errors;

        return response;
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
