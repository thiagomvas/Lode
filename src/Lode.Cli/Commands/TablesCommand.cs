using Lode.Cli;
using Spectre.Console;

namespace Lode.Cli.Commands;

public sealed class TablesCommand : ICliCommand
{
    public string Name => "tables";
    public string Description => "List all tables in the connected database";
    public string Usage => ".tables";
    public bool RequiresConnection => true;

    public async Task Execute(CommandContext context, CliSession session)
    {
        if (!session.IsConnected)
        {
            AnsiConsole.MarkupLine("[red]You must connect first.[/]");
            return;
        }

        var result = await session.Connection.Schema.GetTableNamesAsync();

        var table = new Table();
        table.AddColumn("Tables");

        foreach (var t in result.Data)
            table.AddRow(t);

        AnsiConsole.Write(table);
    }
}