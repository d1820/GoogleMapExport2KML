using GoogleMapExport2KML.Models;
using TinyCsvParser.Mapping;

namespace GoogleMapExport2KML.Mappings;
public class CsvLineItemMapping : CsvMapping<CsvLineItem>
{
    public CsvLineItemMapping()
        : base()
    {
        MapProperty(0, x => x.Title);
        MapProperty(1, x => x.Note);
        MapProperty(2, x => x.URL);
        MapProperty(3, x => x.Comment);
    }
}
