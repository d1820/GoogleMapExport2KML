
using Microsoft.Playwright;

namespace GoogleMapExport2KML.Automation;

public class PlaywrightBrowser : Interfaces.IBrowser
{
    public KmlPlaywrightDriver Driver { get; }

    public string Id { get; } = Guid.NewGuid().ToString();

    private Microsoft.Playwright.IBrowser _browser;
    private IPage _page;

    public PlaywrightBrowser(KmlPlaywrightDriver driver)
    {
        Driver = driver;
    }

    private async Task LoadBrowserAsync()
    {
        _browser = await Driver.Instance.Webkit.LaunchAsync(new() { Headless = true });
        _page = await _browser!.NewPageAsync();
    }

    public string GetUrl()
    {
        return _page.Url;
    }

    public async Task GotoUrlAsync(string url)
    {
        if (_browser is null)
        {
            await LoadBrowserAsync();
        }
        await _page.GotoAsync(url, new PageGotoOptions { Timeout = 0 });
    }

    public void Dispose()
    {
        Driver.Dispose();
    }
}
