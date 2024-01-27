namespace GoogleMapExport2KML.Models;

public class Point
{
    public string Coordinates { get; set; }

    public Point()
    {
    }

    public Point(string coordinates)
    {
        Coordinates = coordinates;
    }
}
