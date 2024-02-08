namespace GoogleMapExport2KML.Interfaces;

public interface IBrowser
{
    string Id { get; }

    Task GotoUrlAsync(string url);

    string GetUrl();
}
