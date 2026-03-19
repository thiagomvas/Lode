using Lode.Core.ValueTypes;
using Spectre.Console;

namespace Lode.Cli.Commands;

public sealed class SchemaCommand : ICliCommand
{
    public string Name => "schema";
    public string Description => "Show the column definitions for a table";
    public string Usage => ".schema <table>";
    public bool RequiresConnection => true;

    public async Task Execute(CommandContext context, CliSession session)
    {
        if (context.Args.Count < 1)
        {
            AnsiConsole.MarkupLine("[red]Usage: .schema <table>[/]");
            return;
        }

        var tableName = context.Args[0];
        var result = await session.Connection.Schema.GetTableDefinitionAsync(tableName);

        if (result.IsFailure)
        {
            AnsiConsole.MarkupLine($"[red]Failed to get schema:[/] {string.Join(", ", result.Errors.Select(e => e.Message))}");
            return;
        }

        var table = new Table().Title($"[yellow]{result.Data.Name}[/]");
        table.AddColumn("Column");
        table.AddColumn("Type");
        table.AddColumn("Nullable");
        table.AddColumn("Flags");

        foreach (var col in result.Data.Columns)
        {
            var nullable = col.Flags.HasFlag(ColumnFlags.Nullable) ? "YES" : "NO";
            var flags = FormatFlags(col.Flags);
            table.AddRow(col.Name, col.Type.ToString(), nullable, flags);
        }

        AnsiConsole.Write(table);
    }

    private static string FormatFlags(ColumnFlags flags)
    {
        var parts = new List<string>();
        if (flags.HasFlag(ColumnFlags.PrimaryKey))   parts.Add("PK");
        if (flags.HasFlag(ColumnFlags.ForeignKey))   parts.Add("FK");
        if (flags.HasFlag(ColumnFlags.UniqueKey))    parts.Add("UQ");
        if (flags.HasFlag(ColumnFlags.AutoIncrement)) parts.Add("AI");
        if (flags.HasFlag(ColumnFlags.Indexed))      parts.Add("IDX");
        if (flags.HasFlag(ColumnFlags.Default))      parts.Add("DEFAULT");
        if (flags.HasFlag(ColumnFlags.Computed))     parts.Add("COMPUTED");
        return parts.Count > 0 ? string.Join(", ", parts) : "-";
    }
}
