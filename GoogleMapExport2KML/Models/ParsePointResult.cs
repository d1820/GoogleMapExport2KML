namespace GoogleMapExport2KML.Models;

public class ParsePointResult
{
    public string ErrorMessage { get; set; }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public Point? Point { get; set; }
}
