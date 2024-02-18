using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GoogleMapExport2KML.Commands;

public class SplitSettings : KmlBaseSettings
{
    [CommandOption("--dryrun")]
    [Description("If true. Runs through the files and estimates times to completion.")]
    public bool DryRun { get; set; }

    [CommandOption("-f|--file")]
    [Description("The kml file to split")]
    public string KmlFile { get; set; }

    [CommandOption("-o|--output")]
    [Description("The output path.")]
    public string OutputPath { get; set; }

    [CommandOption("--placements-per-file")]
    [Description("The number of placements to add per KML file. Files will be named based on number of files needed. Default ALL")]
    public int PlacementsPerFile { get; set; } = -1;

    public override ValidationResult Validate()
    {
        var fi = new FileInfo(KmlFile);
        var nameValid = !string.IsNullOrEmpty(OutputPath);
        if (fi.Exists != true)
        {
            return ValidationResult.Error("KML file can not be found");
        }
        if (!fi.Extension.Equals(".kml", StringComparison.InvariantCultureIgnoreCase))
        {
            return ValidationResult.Error("Invalid file. Only KML files supported");
        }
        if (!nameValid)
        {
            return ValidationResult.Error("output path is required");
        }

        return ValidationResult.Success();
    }
}
