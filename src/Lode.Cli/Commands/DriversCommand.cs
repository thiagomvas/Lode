using Lode.Cli;
using Spectre.Console;
using Lode.Core.Abstractions;

namespace Lode.Cli.Commands;

public sealed class DriversCommand : ICliCommand
{
    private readonly IDriverRegistry _driverRegistry;
    public string Name => "drivers";

    public DriversCommand(IDriverRegistry driverRegistry)
    {
        _driverRegistry = driverRegistry;
    }

    public Task Execute(CommandContext context, CliSession session)
    {
        var table = new Table();
        table.AddColumn("Driver");
        table.AddColumn("Capabilities");

        foreach (var driver in _driverRegistry.GetDrivers())
            table.AddRow([driver.Name, driver.Capabilities.ToString()]);

        AnsiConsole.Write(table);
        return Task.CompletedTask;
    }
}