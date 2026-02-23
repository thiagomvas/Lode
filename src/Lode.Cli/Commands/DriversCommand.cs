using Lode.Cli;
using Spectre.Console;
using Lode.Core.Abstractions;

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

        foreach (var name in _driverRegistry.GetDriverNames())
            table.AddRow(name);

        AnsiConsole.Write(table);
        return Task.CompletedTask;
    }
}