namespace GoogleMapExport2KML.Extensions;

internal static class DisplayExtensions
{
    internal static string MsToTime(this long milliseconds)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    internal static string MsToTime(this int milliseconds)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    internal static string MsToTime(this double milliseconds)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }
}
