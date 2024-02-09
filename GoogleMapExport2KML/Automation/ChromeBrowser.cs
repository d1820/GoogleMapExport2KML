using GoogleMapExport2KML.Interfaces;

namespace GoogleMapExport2KML.Automation;

public class ChromeBrowser : IBrowser
{
    public KmlChromeDriver Driver { get; }

    public string Id => Driver.Id;

    public ChromeBrowser(KmlChromeDriver driver)
    {
        Driver = driver;
    }
    public string GetUrl()
    {
        return Driver.Instance.Url;
    }

    public Task GotoUrlAsync(string url)
    {
        Driver.Instance.Navigate().GoToUrl(url);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Driver.Dispose();
    }
}
