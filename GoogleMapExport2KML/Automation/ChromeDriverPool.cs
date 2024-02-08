using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using GoogleMapExport2KML.Commands;
using GoogleMapExport2KML.Interfaces;
using Spectre.Console;

namespace GoogleMapExport2KML.Automation;

public class ChromeDriverPool : IWebDriverPool
{
    private ParseSettings _settings;
    private Process _proc;
    //private ConcurrentBag<KmlChromeDriver> _drivers = [];
    private ConcurrentBag<KmlChromeDriver> _objectPool = [];

    public Task InitializeAsync(ParseSettings settings)
    {
        _settings = settings;
        _objectPool.Clear();
        for (var i = 0; i < settings.MaxDegreeOfParallelism; i++) // create 1 driver per parallelism
        {
            var driver = new KmlChromeDriver(settings);
            driver.SetPool(this);
            driver.SetTimeouts(_settings.QueryPlacesWaitTimeoutSeconds);
            driver.SetBrowser(new ChromeBrowser(driver));
            _objectPool.Add(driver);
        }
        return Task.CompletedTask;

    }
    public IDriver Get()
    {
        ArgumentNullException.ThrowIfNull(_objectPool, nameof(_objectPool));
        ArgumentNullException.ThrowIfNull(_settings, nameof(_settings));

        _objectPool.TryTake(out var driver);
        if (driver is null)
        {
            driver = new KmlChromeDriver(_settings);
            driver.SetPool(this);
            driver.SetTimeouts(_settings.QueryPlacesWaitTimeoutSeconds);
            driver.SetBrowser(new ChromeBrowser(driver));
            //Add new one to pool
            AnsiConsole.MarkupLine($"[grey50]Adding new Driver to pool from Get() {driver.Id}[/]");
            _objectPool.Add(driver);
        }
        return driver;
    }

    //public async Task ReturnAsync(KmlChromeDriver driver)
    //{
    //    //if (GetChromeProcessUsage() > 3000000000)
    //    //{
    //    //    AnsiConsole.MarkupLine($"[grey50]4GB met[/]");
    //    //    _driversToFlush.Add(driver);
    //    //    //we reach threshold we need to flush all the drivers and start over
    //    //    if (_driversToFlush.Count == _settings.MaxDegreeOfParallelism)
    //    //    {
    //    //        AnsiConsole.MarkupLine($"[grey50]Flushing drivers[/]");
    //    //        foreach (var d in _driversToFlush)
    //    //        {
    //    //            //d.Instance.Close();
    //    //            d.Instance.Quit();
    //    //            //_objectPool.Return(d);
    //    //            await Task.Delay(1000);
    //    //        }
    //    //        AnsiConsole.MarkupLine($"[grey50]Resetting flush driver queue[/]");
    //    //        _driversToFlush.Clear();
    //    //    }
    //    //}
    //    //else
    //    //{
    //    //    _objectPool.Return(driver);
    //    //}
    //    driver.Instance.Close();
    //    _objectPool.Add(driver);
    //}


    public Task ReturnAsync(IBrowser browser)
    {
        var internalBrowser = browser as ChromeBrowser;
        _objectPool.Add(internalBrowser!.Driver);
        return Task.CompletedTask;
    }

    private long GetChromeProcessUsage()
    {
        var chromeProcesses = Process.GetProcessesByName("chrome");
        var filteredProcesses = chromeProcesses.Where(process => process.WorkingSet64 > 500 * 1024 * 1024); //  500MB in bytes
        return filteredProcesses.Sum(s => s.WorkingSet64);
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
                d.Instance.Quit();
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
