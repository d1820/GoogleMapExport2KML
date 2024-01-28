using Microsoft.Extensions.ObjectPool;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace GoogleMapExport2KML.Factories;


public class ChromeDriverPool
{
    private readonly ObjectPool<KmlChromeDriver> _objectPool = new DefaultObjectPool<KmlChromeDriver>(new DefaultPooledObjectPolicy<KmlChromeDriver>());

    public KmlChromeDriver Get()
    {
        var obj = _objectPool.Get();
        obj.SetPool(this);
        return obj;
    }
    public void Return(KmlChromeDriver driver)
    {
        _objectPool.Return(driver);
    }
}

public class KmlChromeDriver : IResettable, IDisposable
{
    private ChromeDriver _driver;
    private bool _disposedValue;
    private ChromeDriverPool _pool;

    public void SetPool(ChromeDriverPool pool)
    {
        _pool = pool;
    }

    public KmlChromeDriver()
    {
        ChromeOptions options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--headless");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-crash-reporter");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-in-process-stack-traces");
        options.AddArgument("--disable-logging");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--log-level=3");
        options.AddArgument("--output=/dev/null");
        options.SetLoggingPreference(LogType.Driver, LogLevel.Off);
        options.SetLoggingPreference(LogType.Browser, LogLevel.Off);
        options.SetLoggingPreference(LogType.Client, LogLevel.Off);
        options.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
        options.SetLoggingPreference(LogType.Server, LogLevel.Off);
        var svc = ChromeDriverService.CreateDefaultService();
        svc.SuppressInitialDiagnosticInformation = true;
        svc.HideCommandPromptWindow = true;
        svc.DisableBuildCheck = true;

        _driver = new ChromeDriver(svc, options);
    }

    public ChromeDriver Instance => _driver;

    public bool TryReset()
    {
        _driver?.ClearNetworkConditions();
        return true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _pool?.Return(this);
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
