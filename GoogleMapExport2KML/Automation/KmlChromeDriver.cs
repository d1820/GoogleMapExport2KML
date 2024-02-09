using GoogleMapExport2KML.Commands;
using GoogleMapExport2KML.Interfaces;
using Microsoft.Extensions.ObjectPool;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Spectre.Console;

namespace GoogleMapExport2KML.Automation;

public class KmlChromeDriver : IResettable, IDriver
{
    private ChromeDriverPool _pool;
    private readonly ParseSettings _settings;

    public IBrowser Browser { get; private set; }

    public string Id { get; }

    public ChromeDriver Instance { get; }

    public KmlChromeDriver(ParseSettings settings)
    {
        Id = Guid.NewGuid().ToString();
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--incognito");
        options.AddArgument("--no-zygote");
        options.AddArgument("--disk-cache-size=1");
        options.AddArgument("--no-sandbox");

        options.AddArgument("--disable-background-networking");
        options.AddArgument("--disable-default-apps");
        options.AddArgument("--disable-sync");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-crash-reporter");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-in-process-stack-traces");
        options.AddArgument("--disable-logging");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--log-level=3");
        options.AddArgument("--output=/dev/null");
        options.SetLoggingPreference(LogType.Driver, LogLevel.Severe);
        options.SetLoggingPreference(LogType.Browser, LogLevel.Off);
        options.SetLoggingPreference(LogType.Client, LogLevel.Off);
        options.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
        options.SetLoggingPreference(LogType.Server, LogLevel.Severe);
        options.PageLoadStrategy = PageLoadStrategy.Eager;
        var svc = ChromeDriverService.CreateDefaultService();
        svc.SuppressInitialDiagnosticInformation = true;
        svc.HideCommandPromptWindow = true;
        svc.DisableBuildCheck = true;

        Instance = new ChromeDriver(svc, options);
        if (settings.Trace)
        {
            AnsiConsole.MarkupLine($"Created driver instance {Id}");
        }
        _settings = settings;
    }

    public void SetTimeouts(double timeout)
    {
        Instance.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(timeout);
        Instance.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(timeout);
    }

    public void SetBrowser(IBrowser browser)
    {
        Browser = browser;
    }

    public void SetPool(ChromeDriverPool pool)
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
