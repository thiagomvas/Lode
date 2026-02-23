using Spectre.Console;
using System;
using System.Threading.Tasks;

namespace Lode.Cli;

public sealed class InteractiveCli
{
    private readonly CommandRegistry _registry;
    private readonly CliSession _session;

    public InteractiveCli(CommandRegistry registry)
    {
        _registry = registry;
        _session = new CliSession();
    }

    public async Task Run()
    {
        AnsiConsole.MarkupLine("[green]Lode CLI Interactive[/]");
        AnsiConsole.MarkupLine("Type 'help' for commands or 'exit' to quit.");

        while (true)
        {
            var prompt = _session.IsConnected 
                ? $"[yellow]{_session.Connection.FormattedName}>[/] "
                : "[yellow]disconnected>[/] ";

            var input = AnsiConsole.Ask<string>(prompt);

            if (string.IsNullOrWhiteSpace(input)) continue;
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

            var (commandName, context) = CommandParser.Parse(input);
            if (commandName == null) continue;

            if (!_registry.TryGet(commandName, out var command))
            {
                AnsiConsole.MarkupLine($"[red]Unknown command:[/] {commandName}");
                continue;
            }

            if (!_session.IsConnected && commandName != "connect" && commandName != "help" && commandName != "drivers")
            {
                AnsiConsole.MarkupLine("[red]You must connect first using 'connect <driver> ...'[/]");
                continue;
            }

            try
            {
                await command.Execute(context, _session);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
        }
    }
}