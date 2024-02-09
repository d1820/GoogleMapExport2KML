namespace GoogleMapExport2KML.Models;

public class Stat
{
    public string Event { get; set; }

    public long TotalMs { get; set; }

    public Stat(string @event, long totalMs)
    {
        Event = @event;
        TotalMs = totalMs;
    }
}
