using Lode.Core.Abstractions;
using Lode.Core.ValueTypes;
using Spectre.Console;
using System.Threading.Tasks;
using Lode.Business;

namespace Lode.Cli.Commands;

public sealed class ExportCommand : ICliCommand
{
    public string Name { get; } = "export";
    private readonly IDriverRegistry _driverRegistry;

    public ExportCommand(IDriverRegistry driverRegistry)
    {
        _driverRegistry = driverRegistry;
    }


    public async Task Execute(CommandContext context, CliSession session)
    {
        if (!session.IsConnected)
        {
            AnsiConsole.MarkupLine("[red]You must be connected to a source database first.[/]");
            return;
        }

        if (!context.Options.TryGetValue("target-driver", out var targetDriverName))
        {
            AnsiConsole.MarkupLine("[red]Missing target driver. Use --target-driver <driver>[/]");
            return;
        }

        if (!_driverRegistry.TryGetDriver(targetDriverName, out var targetDriver))
        {
            AnsiConsole.MarkupLine("[red]Invalid target driver.[/]");
            return;
        }

        if (!context.Options.TryGetValue("target-connection", out var targetConnectionString))
        {
            AnsiConsole.MarkupLine("[red]Missing target connection string. Use --target-connection <connection>[/]");
            return;
        }

        var tables = context.Args;

        AnsiConsole.MarkupLine($"[green]Exporting from {session.Connection.FormattedName} to {targetDriverName}...[/]");


        var migration = new MigrationService();
        var targetOpt = targetDriver.BuildOptionsFromConnectionString(targetConnectionString);
        var targetConnection = await targetDriver.OpenConnectionAsync(targetOpt);

        await migration.MigrateAsync(session.Connection, targetConnection.Data, tables);

        AnsiConsole.MarkupLine("[green]Export complete.[/]");
    }
}