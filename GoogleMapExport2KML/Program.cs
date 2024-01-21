// See https://aka.ms/new-console-template for more information

using GoogleMapExport2KML.Commands;
using GoogleMapExport2KML.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

var registrations = new ServiceCollection();
registrations.AddSingleton<CsvParser>();
registrations.AddSingleton<KMLService>();




// Create a type registrar and register any dependencies.
// A type registrar is an adapter for a DI framework.
var registrar = new TypeRegistrar(registrations);

AnsiConsole.Write(
    new FigletText("GoogleMapExport2KML")
        .LeftJustified()
        .Color(Color.Blue));

Console.WriteLine("");
var panel = new Panel("Take that Google for not making this easy");
panel.Border = BoxBorder.Double;
panel.Padding = new Padding(2, 2, 2, 2);
panel.Expand = false;
AnsiConsole.Write(panel);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.AddCommand<ParseCommand>("parse")
        .WithDescription("Parses .csv files generated from a google maps export of Saved Places")
        .WithExample("parse",@"-f=C:\downloads\myplaces.csv", @"-f=C:\downloads\myfavoriteplaces.csv", "-o=MyCombinedPlaces.kml");
#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});
args = ["parse", @"-f=C:\Users\d1820\Downloads\takeout-20240120T191337Z-001\Takeout\Saved\Camping.csv", "-o=Output/test.kml"];

await app.RunAsync(args);
