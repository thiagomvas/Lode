using Spectre.Console;

namespace Lode.Cli.Commands;

public sealed class QueryCommand : ICliCommand
{
    public string Name { get; } = "query";
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

        var result = await session.Connection.Query.ExecuteQueryAsync(query);

        if (result.IsFailure)
        {
            AnsiConsole.MarkupLine($"[red]Query failed:[/] {query}");
            return;
        }

        var table = new Table();
        foreach (var column in result.Data.Columns)
            table.AddColumn(column.Name);

        foreach (var row in result.Data.Rows)
        {
            var stringRow = row.Select(cell => cell?.ToString() ?? "NULL").ToArray();
            table.AddRow(stringRow);
        }

        AnsiConsole.Write(table);
    }
}
