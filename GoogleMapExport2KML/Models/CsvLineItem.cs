namespace GoogleMapExport2KML.Models;

public class CsvLineItem
{
    public string Comment { get; set; }

    public string DisplayName
    {
        get
        {
            if (URL.Contains("https://www.google.com/maps/search/"))
            {
                return URL.Replace("https://www.google.com/maps/search/", "");
            }

            if (URL.Contains("https://www.google.com/maps/place/"))
            {
                //coords: @38.6117469,-106.3202388,17z
                return URL.Replace("https://www.google.com/maps/place/", "").Split("/").First().Replace("+", " ");
            }
            return string.Empty;
        }
    }

    public string Note { get; set; }

    public int RowNumber { get; set; }

    public string Title { get; set; }

    public string URL { get; set; }
}
