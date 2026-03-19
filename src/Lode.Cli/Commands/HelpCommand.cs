using Spectre.Console;

namespace Lode.Cli.Commands;

public sealed class HelpCommand : ICliCommand
{
    private readonly CommandRegistry _registry;
    public string Name => "help";
    public string Description => "List all available commands";
    public string Usage => ".help";
    public bool RequiresConnection => false;

    public HelpCommand(CommandRegistry registry)
    {
        _registry = registry;
    }

    public Task Execute(CommandContext context, CliSession session)
    {
        var table = new Table();
        table.AddColumn("Command");
        table.AddColumn("Description");
        table.AddColumn("Usage");

        foreach (var command in _registry.GetCommands())
            table.AddRow(command.Name, command.Description, command.Usage);

        AnsiConsole.Write(table);
        return Task.CompletedTask;
    }
}