namespace GoogleMapExport2KML.Interfaces;

public interface IBrowser: IDisposable
{
    string Id { get; }

    Task GotoUrlAsync(string url);

    string GetUrl();
}
