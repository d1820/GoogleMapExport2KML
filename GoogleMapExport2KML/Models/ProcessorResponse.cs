namespace GoogleMapExport2KML.Models;
public class ProcessorResponse
{
    public List<CsvLineItemError> Errors { get; set; } = [];
    public bool IsSuccess => Errors?.Any() != true;
    public List<Placemark> Placemarks { get; set; } = [];
}
