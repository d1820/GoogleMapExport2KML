using System.ComponentModel;
using Spectre.Console.Cli;

namespace GoogleMapExport2KML.Commands;
public class KmlBaseSettings: CommandSettings
{
    [CommandOption("--noheader")]
    [Description("If true. Does not display the banner on command execute")]
    public bool NoHeader { get; set; }
}
