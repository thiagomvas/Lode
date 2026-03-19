using Lode.Core.Models;
using Spectre.Console;

namespace Lode.Cli;

internal static class ResultRenderer
{
    public static void Render(QueryResult result, TimeSpan elapsed)
    {
        if (!result.Columns.Any())
        {
            AnsiConsole.MarkupLine($"[green]Command executed successfully.[/] [grey]({elapsed.TotalMilliseconds:F0}ms)[/]");
            return;
        }

        var table = new Table();
        foreach (var column in result.Columns)
            table.AddColumn(column.Name);

        foreach (var row in result.Rows)
            table.AddRow(row.Select(cell => cell?.ToString() ?? "NULL").ToArray());

        AnsiConsole.Write(table);

        var rowLabel = result.TotalRows == 1 ? "row" : "rows";
        AnsiConsole.MarkupLine($"[grey]{result.TotalRows} {rowLabel} ({elapsed.TotalMilliseconds:F0}ms)[/]");
    }
}
