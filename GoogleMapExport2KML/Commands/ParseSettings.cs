using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GoogleMapExport2KML.Commands;

public class ParseSettings : KmlBaseSettings
{
    [CommandOption("--dryrun")]
    [Description("If true. Runs through the files and estimates times to completion.")]
    public bool DryRun { get; set; }

    [CommandOption("-f|--file <VALUES>")]
    [Description("The csv files to parse")]
    public string[] Files { get; set; }

    [CommandOption("--includeComments")]
    [Description("If true. Adds any comment from the csv column to the description")]
    public bool IncludeCommentInDescription { get; set; }

    [CommandOption("-s|--stats")]
    [Description("If true. Outputs all the timing stats")]
    public bool IncludeStats { get; set; }

    [CommandOption("-p|--parallel")]
    [Description("The number of threads used to process Google data locations. Default 4")]
    public int MaxDegreeOfParallelism { get; set; } = 4;

    [CommandOption("-b|--batch")]
    [Description("The number of items to group into a batch. Default 10")]
    public int BatchCount { get; set; } = 10;

    [CommandOption("-o|--output")]
    [Description("The output KML file.")]
    public string OutputFile { get; set; }

    [CommandOption("--placements-per-file")]
    [Description("The number of placements to add per KML file. Files will be named based on number of files needed. Default ALL")]
    public int PlacementsPerFile { get; set; } = -1;

    [CommandOption("-t|--timeout")]
    [Description("The timeout to wait on each lookup for coordinates from Google. Default 10s")]
    public double QueryPlacesTimeoutSeconds { get; set; } = 10;

    //[CommandOption("-w|--wait-timeout")]
    //[Description("The timeout to wait for the page to load from Google. Default 300s")]
    //public double QueryPlacesWaitTimeoutSeconds { get; set; } = 300;

    [CommandOption("--stopOnError")]
    [Description("If true. Stops parsing on any csv row error.")]
    public bool StopOnError { get; set; }

    [CommandOption("-v|--verbose")]
    [Description("If true. Increases the level of the output")]
    public bool Verbose { get; set; }

    [CommandOption("--trace")]
    [Description("If true. Outputs all the tracing for each processing line")]
    public bool Trace { get; set; }

    public override ValidationResult Validate()
    {
        var filesValid = Files?.All(File.Exists);
        var nameValid = !string.IsNullOrEmpty(OutputFile);
        if (filesValid != true)
        {
            return ValidationResult.Error("One or more files can not be found");
        }
        if (!nameValid)
        {
            return ValidationResult.Error("output file is required");
        }

        return ValidationResult.Success();
    }
}
