namespace GoogleMapExport2KML.Models;

public class PlacemarkResult
{
    public string ErrorMessage { get; set; }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public Placemark? Placemark { get; set; }
}
