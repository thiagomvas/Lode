using System.Diagnostics;
using Spectre.Console;

namespace Lode.Cli.Commands;

public sealed class QueryCommand : ICliCommand
{
    public string Name => "query";
    public string Description => "Execute a SQL query and display the results";
    public string Usage => ".query <sql>";
    public bool RequiresConnection => true;
    public async Task Execute(CommandContext context, CliSession session)
    {
        if (!session.IsConnected)
        {
            AnsiConsole.MarkupLine("[red]You must connect first using --driver / --connection[/]");
            return;
        }

        // The full query is passed as positional arguments
        if (context.Args == null || !context.Args.Any())
        {
            AnsiConsole.MarkupLine("[red]No query specified.[/]");
            return;
        }

        var query = string.Join(" ", context.Args);

        var sw = Stopwatch.StartNew();
        var result = await session.Connection.Query.ExecuteQueryAsync(query);
        sw.Stop();

        if (result.IsFailure)
        {
            AnsiConsole.MarkupLine($"[red]Query failed:[/] {string.Join(", ", result.Errors.Select(e => e.Message))}");
            return;
        }

        ResultRenderer.Render(result.Data, sw.Elapsed);
    }
}
