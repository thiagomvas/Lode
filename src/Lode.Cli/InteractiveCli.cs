using System.Diagnostics;
using Spectre.Console;

namespace Lode.Cli;

public sealed class InteractiveCli
{
    private readonly CommandRegistry _registry;
    private readonly CliSession _session;
    private string _lastCommand = string.Empty;

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

            string input = AnsiConsole.Ask<string>(prompt)?.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                if (string.IsNullOrWhiteSpace(_lastCommand)) continue;
                input = _lastCommand;
            }
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

            if (input.StartsWith("."))
            {
                input = input.TrimStart('.');
                var (commandName, context) = CommandParser.Parse(input);
                if (commandName == null) continue;

                if (!_registry.TryGet(commandName, out var command))
                {
                    AnsiConsole.MarkupLine($"[red]Unknown command:[/] {commandName}");
                    continue;
                }

                if (!_session.IsConnected && command.RequiresConnection)
                {
                    AnsiConsole.MarkupLine("[red]You must connect first using '.connect <driver> ...'[/]");
                    continue;
                }

                _lastCommand = "." + input;
                try
                {
                    await command.Execute(context, _session);
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteException(ex);
                }

                continue;
            }
            if (!_session.IsConnected)
            {
                AnsiConsole.MarkupLine("[red]You must connect first using 'connect <driver> ...' before querying[/]");
                continue;
            }

            while (!input.TrimEnd().EndsWith(";"))
            {
                var nextLine = AnsiConsole.Ask<string>("…> ")?.Trim();
                if (string.IsNullOrWhiteSpace(nextLine)) continue;
                input += "\n" + nextLine;
            }

            _lastCommand = input;
            var sw = Stopwatch.StartNew();
            var queryResult = await _session.Connection.Query.ExecuteQueryAsync(input);
            sw.Stop();

            if (queryResult.IsFailure)
            {
                AnsiConsole.MarkupLine($"[red]Query failed:[/] {string.Join(", ", queryResult.Errors.Select(e => e.Message))}");
                continue;
            }

            ResultRenderer.Render(queryResult.Data, sw.Elapsed);
        }
    }
}