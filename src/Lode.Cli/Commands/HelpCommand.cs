using Spectre.Console;

namespace Lode.Cli.Commands;

public sealed class HelpCommand : ICliCommand
{
    private readonly CommandRegistry _registry;
    public string Name => "help";

    public HelpCommand(CommandRegistry registry)
    {
        _registry = registry;
    }

    public Task Execute(CommandContext context, CliSession session)
    {
        var table = new Table();
        table.AddColumn("Command");

        foreach (var command in _registry.GetCommands())
            table.AddRow(command.Name);

        AnsiConsole.Write(table);
        return Task.CompletedTask;
    }
}