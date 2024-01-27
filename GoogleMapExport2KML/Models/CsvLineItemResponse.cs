namespace GoogleMapExport2KML.Models;

public class CsvLineItemResponse
{
    public List<CsvLineItemError> Errors { get; set; } = new List<CsvLineItemError>();

    public bool HasErrors => Errors?.Any() == true;

    public List<CsvLineItem> Results { get; set; } = new List<CsvLineItem>();
}
