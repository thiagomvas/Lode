using Spectre.Console;

namespace Lode.Cli.Commands;

public sealed class ClearCommand : ICliCommand
{
    public string Name => "clear";
    public string Description => "Clear the terminal screen";
    public string Usage => ".clear";
    public bool RequiresConnection => false;

    public Task Execute(CommandContext context, CliSession session)
    {
        AnsiConsole.Clear();
        return Task.CompletedTask;
    }
}
