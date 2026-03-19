using System.Text;
using System.Text.Json;
using Lode.Core.Models;
using Spectre.Console;

namespace Lode.Cli;

public enum OutputFormat { Table, Csv, Json }

internal static class ResultRenderer
{
    public static void Render(QueryResult result, TimeSpan elapsed, OutputFormat format = OutputFormat.Table)
    {
        if (!result.Columns.Any())
        {
            if (format == OutputFormat.Table)
                AnsiConsole.MarkupLine($"[green]Command executed successfully.[/] [grey]({elapsed.TotalMilliseconds:F0}ms)[/]");
            return;
        }

        switch (format)
        {
            case OutputFormat.Csv:
                RenderCsv(result);
                break;
            case OutputFormat.Json:
                RenderJson(result);
                break;
            default:
                RenderTable(result, elapsed);
                break;
        }
    }

    private static void RenderTable(QueryResult result, TimeSpan elapsed)
    {
        var table = new Table();
        foreach (var column in result.Columns)
            table.AddColumn(column.Name);

        foreach (var row in result.Rows)
            table.AddRow(row.Select(cell => cell?.ToString() ?? "NULL").ToArray());

        AnsiConsole.Write(table);

        var rowLabel = result.TotalRows == 1 ? "row" : "rows";
        AnsiConsole.MarkupLine($"[grey]{result.TotalRows} {rowLabel} ({elapsed.TotalMilliseconds:F0}ms)[/]");
    }

    private static void RenderCsv(QueryResult result)
    {
        var sb = new StringBuilder();

        sb.AppendLine(string.Join(",", result.Columns.Select(c => EscapeCsv(c.Name))));
        foreach (var row in result.Rows)
            sb.AppendLine(string.Join(",", row.Select(cell => EscapeCsv(cell?.ToString() ?? ""))));

        Console.Write(sb);
    }

    private static void RenderJson(QueryResult result)
    {
        var columnNames = result.Columns.Select(c => c.Name).ToList();

        var rows = result.Rows.Select(row =>
        {
            var dict = new Dictionary<string, string?>();
            for (int i = 0; i < columnNames.Count; i++)
                dict[columnNames[i]] = row[i]?.ToString();
            return dict;
        }).ToList();

        var json = JsonSerializer.Serialize(rows, typeof(List<Dictionary<string, string?>>), LodeJsonContext.Default);
        Console.WriteLine(json);
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
