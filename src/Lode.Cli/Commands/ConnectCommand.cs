using Lode.Core.Abstractions;
using Lode.Cli;
using Spectre.Console;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lode.Cli.Commands;

public sealed class ConnectCommand : ICliCommand
{
    private readonly IDriverRegistry _driverRegistry;
    public string Name => "connect";
    public string Description => "Connect to a database using the specified driver";
    public string Usage => ".connect <driver> [key=value ...]";
    public bool RequiresConnection => false;

    public ConnectCommand(IDriverRegistry driverRegistry)
    {
        _driverRegistry = driverRegistry;
    }

    public async Task Execute(CommandContext context, CliSession session)
    {
        if (context.Args.Count < 1)
        {
            AnsiConsole.MarkupLine("[red]Usage: connect <driver> [key=value ...][/]");
            return;
        }

        var driverName = context.Args[0];

        if (!_driverRegistry.TryGetDriver(driverName, out var driver))
        {
            AnsiConsole.MarkupLine($"[red]Driver '{driverName}' not found[/]");
            return;
        }

        var options = driver.GetDefaultOptions();

        foreach (var arg in context.Args.Skip(1))
        {
            var kv = arg.Split('=', 2);
            if (kv.Length != 2) continue;

            var key = kv[0].Trim();
            var value = kv[1].Trim();

            if (string.Equals(key, "File", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "FilePath", StringComparison.OrdinalIgnoreCase))
            {
                options.FilePath = value;
                continue;
            }

            options.Options[key] = value;
        }

        try
        {
            var result = await driver.OpenConnectionAsync(options);
            if (result.IsFailure)
            {
                AnsiConsole.MarkupLine($"[red]Failed to connect:[/] {string.Join(',', result.Errors)}");
                return;
            }

            session.Driver = driver;
            session.Connection = result.Data;
            session.Options = options;

            AnsiConsole.MarkupLine($"[green]Connected to {driver.Name}[/]");
        }
        catch (Exception ex)
        {
            session.Driver = null;
            session.Connection = null;
            session.Options = null;
            AnsiConsole.MarkupLine($"[red]Connection failed:[/] {ex.Message}");
        }
    }
}