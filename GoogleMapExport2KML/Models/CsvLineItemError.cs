namespace GoogleMapExport2KML.Models;

public class CsvLineItemError
{
    public int ColumnIndex { get; set; }

    public string Error { get; set; }

    public DateTime ErrorDate { get; } = DateTime.Now;

    public string Row { get; set; }

    public int RowIndex { get; set; }

    public override string ToString()
    {
        return $"{ErrorDate} - Row: {RowIndex}. column: {ColumnIndex}. Error: {Error}. Data: [{Row}]";
    }
}
