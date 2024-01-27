using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace GoogleMapExport2KML.Factories;
public class ChromeFactory
{
    public ChromeDriver CreateDriver()
    {
        //Set ChromeOptions to run the browser in headless mode
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
        return new ChromeDriver(svc, options);
    }
}
