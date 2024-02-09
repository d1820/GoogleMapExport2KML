using CsvHelper.Configuration;

namespace GoogleMapExport2KML.Models;

public sealed class CsvLineItemMap : ClassMap<CsvLineItem>
{
    public CsvLineItemMap()
    {
        Map(m => m.Title);
        Map(m => m.Note);
        Map(m => m.URL);
        Map(m => m.Comment);
        Map(m => m.RowNumber).Convert(row => row.Row.Parser.RawRow);
    }
}
