using Lode.Cli;
using Spectre.Console;

public sealed class TablesCommand : ICliCommand
{
    public string Name => "tables";

    public async Task Execute(CommandContext context, CliSession session)
    {
        var result = await session.Connection.Schema.GetTableNamesAsync(); // implement in your IDbConnection

        var table = new Table();
        table.AddColumn("Tables");

        foreach (var t in result.Data)
            table.AddRow(t);

        AnsiConsole.Write(table);
    }
}