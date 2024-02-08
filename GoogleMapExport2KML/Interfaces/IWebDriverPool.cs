using GoogleMapExport2KML.Commands;

namespace GoogleMapExport2KML.Interfaces;
public interface IWebDriverPool
{
    IDriver Get();
    Task InitializeAsync(ParseSettings settings);
    Task ReturnAsync(IBrowser driver);
    Task ShutDownDriversAsync();
}
