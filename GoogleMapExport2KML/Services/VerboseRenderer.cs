using Spectre.Console;

namespace GoogleMapExport2KML.Commands;

public class VerboseRenderer
{
    public void Render(ParseCommand.ParseSettings settings, Action action)
    {
        if (settings.Verbose)
        {
            AnsiConsole.MarkupLine("[yellow bold]---------------------------------[/]");
        }
        action.Invoke();
        if (settings.Verbose)
        {
            Console.WriteLine("");
        }
    }
}
