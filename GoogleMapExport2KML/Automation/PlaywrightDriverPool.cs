using System.Collections.Concurrent;
using System.Diagnostics;
using GoogleMapExport2KML.Commands;
using GoogleMapExport2KML.Interfaces;
using Microsoft.Playwright;
using Spectre.Console;

namespace GoogleMapExport2KML.Automation;

public class PlaywrightDriverPool : IWebDriverPool
{
    private ParseSettings _settings;
    //private ObjectPool<KmlChromeDriver> _objectPool;
    private Process _proc;
    private ConcurrentBag<KmlPlaywrightDriver> _objectPool = [];

    public async Task InitializeAsync(ParseSettings settings)
    {
        _settings = settings;
        _objectPool.Clear();
        for (var i = 0; i < settings.MaxDegreeOfParallelism; i++) //create 1 driver per parallelism
        {
            var playwright = await Playwright.CreateAsync();
            //var browser = await playwright.Webkit.LaunchAsync(new() { Headless = true });
            //var page = await browser.NewPageAsync();
            //await page.GotoAsync("https://playwright.dev/dotnet");
            //await page.ScreenshotAsync(new() { Path = "screenshot.png" });
            var driver = new KmlPlaywrightDriver(playwright, settings);
            driver.SetPool(this);
            driver.SetBrowser(new PlaywrightBrowser(driver));
            _objectPool.Add(driver);
        }
    }
    public IDriver Get()
    {
        ArgumentNullException.ThrowIfNull(_objectPool, nameof(_objectPool));
        ArgumentNullException.ThrowIfNull(_settings, nameof(_settings));

        _objectPool.TryTake(out var driver);
        if (driver is null)
        {
            AnsiConsole.WriteLine("DRIVER IS NULL");
        }
        return driver;
    }

    public Task ReturnAsync(Interfaces.IBrowser browser)
    {
        var internalBrowser = browser as PlaywrightBrowser;
        _objectPool.Add(internalBrowser!.Driver);
        return Task.CompletedTask;
    }

    public Task ShutDownDriversAsync()
    {
        if (_settings.Verbose)
        {
            AnsiConsole.MarkupLine($"Shutting down {_objectPool.Count} drivers and releasing memory");
        }
        foreach (var d in _objectPool)
        {
            try
            {
                if (_settings.Trace)
                {
                    AnsiConsole.MarkupLine($"[grey50]Shutting down {d.Id}[/]");
                }
                d.Instance.Dispose();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Unable to shutdown {d.Id}. Error: {ex.Message}[/]");
            }
        }
        _objectPool.Clear();
        GC.Collect();
        if (_settings.Verbose)
        {
            AnsiConsole.MarkupLine($"Driver shutdown complete. Memory released");
        }
        return Task.CompletedTask;
    }
}
