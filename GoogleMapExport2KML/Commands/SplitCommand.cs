using GoogleMapExport2KML.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GoogleMapExport2KML.Commands;

public class SplitCommand : AsyncCommand<SplitSettings>
{
    private readonly KMLService _kmlService;

    public SplitCommand(KMLService kmlService)
    {
        _kmlService = kmlService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, SplitSettings settings)
    {
        var kml = _kmlService.ParseKmlFile(settings.KmlFile);
        if (kml is null)
        {
            AnsiConsole.MarkupLine($"[red]Unable to read and parse KML file {settings.KmlFile}[/]");
            return 1;
        }
        var outputFileInfo = new DirectoryInfo(settings.OutputPath);

        if (settings.DryRun)
        {
            var rule = new Rule("[bold darkorange3]DRY RUN[/]");
            rule.Centered();
            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine("");
            var numOfFiles = kml.Document.Placemarks.Chunk(settings.PlacementsPerFile);
            AnsiConsole.MarkupLine($"{numOfFiles.Count()} file(s) would be written to [yellow]{outputFileInfo.FullName}[/]");
        }
        else
        {
            EnsureParentDirectories(outputFileInfo.FullName, settings);
            var outputFilePath = outputFileInfo.FullName;
            _kmlService.SplitIntoFiles(kml, outputFileInfo, settings.PlacementsPerFile, outputFilePath, settings.Trace);

            AnsiConsole.WriteLine("");
            AnsiConsole.MarkupLine($"[green bold]KML file successfully split.[/]");
            AnsiConsole.WriteLine("");
            AnsiConsole.MarkupLine($"File(s) written to [yellow]{outputFileInfo.FullName}[/]");
        }

        return 0;
    }

    private static void EnsureParentDirectories(string directoryPath, KmlBaseSettings settings)
    {
        // Get the directory path without the file name
        var directoryInfo = new DirectoryInfo(directoryPath);

        // Create all necessary parent directories
        if (!string.IsNullOrEmpty(directoryPath) && !directoryInfo.Exists)
        {
            Directory.CreateDirectory(directoryPath);
            if (settings.Trace)
            {
                AnsiConsole.WriteLine($"Parent directories created: {directoryPath}");
            }
        }
    }
}
