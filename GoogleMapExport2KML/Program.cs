// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using GoogleMapExport2KML.Automation;
using GoogleMapExport2KML.Commands;
using GoogleMapExport2KML.Interfaces;
using GoogleMapExport2KML.Mappings;
using GoogleMapExport2KML.Processors;
using GoogleMapExport2KML.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

var registrations = new ServiceCollection();
registrations.AddSingleton<CsvProcessor>();
registrations.AddSingleton<KMLService>();
//registrations.AddSingleton<IWebDriverPool,ChromeDriverPool>();
registrations.AddSingleton<IWebDriverPool, PlaywrightDriverPool>();
registrations.AddSingleton<Mapper>();
registrations.AddSingleton<GeolocationProcessor>();
registrations.AddSingleton<DataLocationProcessor>();
registrations.AddSingleton<StatExecutor>();
registrations.AddSingleton<VerboseRenderer>();

// Create a type registrar and register any dependencies. A type registrar is an adapter for a DI framework.
var registrar = new TypeRegistrar(registrations);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.SetApplicationName("goog2kml");

    config.AddCommand<ParseCommand>("parse")
        .WithDescription("Parses .csv files generated from a google maps export of Saved Places")
        .WithExample("parse", @"-f=C:\downloads\myplaces.csv", @"-f=C:\downloads\myfavoriteplaces.csv", "-o=MyCombinedPlaces.kml");
#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});
if (Debugger.IsAttached)
{
    args = ["parse",
        @"-f=C:\Users\d1820\Downloads\takeout-20240120T191337Z-001\Takeout\Saved\Done.csv",
        "--stopOnError",
        //"--noheader",
        //"--dryrun",
        "--stats",
        "--parallel=10",
        "-t=120",
        "-b=10",
        //"--trace",
        "--verbose",
        //"-c=3",
        @"-o=Output\Done.kml"];

    //args = ["parse", "--help"];

    AnsiConsole.WriteLine($"Running: {string.Join(" ", args)}");
}
if (!args.Contains("--noheader"))
{
    AnsiConsole.Write(
    new FigletText("GoogleMapExport2KML")
        .LeftJustified()
        .Color(Color.Blue));

    AnsiConsole.WriteLine("");
    AnsiConsole.WriteLine("");
    AnsiConsole.Markup("Take this [bold blue]G[/][bold red]O[/][bold yellow]O[/][bold blue]G[/][bold green]L[/][bold red]E[/] for not making this easy ");
    AnsiConsole.WriteLine("");
    AnsiConsole.WriteLine("");
}
await app.RunAsync(args);

if (Debugger.IsAttached)
{
    Console.ReadLine();
}
