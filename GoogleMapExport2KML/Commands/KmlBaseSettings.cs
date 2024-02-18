using System.ComponentModel;
using Spectre.Console.Cli;

namespace GoogleMapExport2KML.Commands;

public class KmlBaseSettings : CommandSettings
{
    [CommandOption("--noheader")]
    [Description("If true. Does not display the banner on command execute")]
    public bool NoHeader { get; set; }

    [CommandOption("--stopOnError")]
    [Description("If true. Stops parsing on any csv row error.")]
    public bool StopOnError { get; set; }

    [CommandOption("-v|--verbose")]
    [Description("If true. Increases the level of the output")]
    public bool Verbose { get; set; }

    [CommandOption("--trace")]
    [Description("If true. Outputs all the tracing for each processing line")]
    public bool Trace { get; set; }
}
