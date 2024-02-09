using System.Runtime;
using GoogleMapExport2KML.Commands;
using GoogleMapExport2KML.Interfaces;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Playwright;
using Spectre.Console;

namespace GoogleMapExport2KML.Automation;

public class KmlPlaywrightDriver : IResettable, IDriver
{
    private PlaywrightDriverPool _pool;
    private readonly ParseSettings _settings;

    public Interfaces.IBrowser Browser { get; private set; }

    public string Id { get; }

    public IPlaywright Instance { get; }

    public KmlPlaywrightDriver(IPlaywright playwright, ParseSettings settings)
    {
        Id = Guid.NewGuid().ToString();
        Instance = playwright;
        _settings = settings;
        if (settings.Trace)
        {
            AnsiConsole.MarkupLine($"Created driver instance {Id}");
        }
    }

    public void SetTimeouts(double timeout)
    {

    }

    public void SetBrowser(Interfaces.IBrowser browser)
    {
        Browser = browser;
    }

    public void SetPool(PlaywrightDriverPool pool)
    {
        _pool = pool;
    }

    public bool TryReset()
    {
        return true;
    }

    public void Dispose()
    {
        if (_settings.Trace)
        {
            AnsiConsole.MarkupLine($"Returning {Id}");
        }
        _pool!.ReturnAsync(Browser).Wait();
    }
}
