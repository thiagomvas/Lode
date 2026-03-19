namespace Lode.Cli.Commands;

using Spectre.Console;

public sealed class DisconnectCommand : ICliCommand
{
    public string Name => "disconnect";
    public string Description => "Close the current database connection";
    public string Usage => ".disconnect";
    public bool RequiresConnection => true;

    public async Task Execute(CommandContext context, CliSession session)
    {
        if (!session.IsConnected)
        {
            AnsiConsole.MarkupLine("[red]Not connected[/]");
            return ;
        }

        await session.Connection.DisposeAsync();
        session.Connection = null;
        session.Driver = null;
        session.Options = null;

        AnsiConsole.MarkupLine("[green]Disconnected[/]");
    }
}