using GoogleMapExport2KML.Commands;
using Spectre.Console;

namespace GoogleMapExport2KML.Services;

public class VerboseRenderer
{
    public void Render(ParseSettings settings, Action action)
    {
        if (settings.Verbose || settings.Trace)
        {
            AnsiConsole.WriteLine("");
            AnsiConsole.MarkupLine("[yellow bold]---------------------------------[/]");
        }
        action.Invoke();
        if (settings.Verbose || settings.Trace)
        {
            AnsiConsole.MarkupLine("[yellow bold]---------------------------------[/]");
            AnsiConsole.WriteLine("");
        }
    }
}
