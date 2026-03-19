using Lode.Business;
using Lode.Cli;
using Lode.Cli.Commands;
using Lode.Core.Abstractions;
using Lode.Drivers.AccessDb;
using Lode.Drivers.Sqlite;
using Spectre.Console;

var driverRegistry = new DriverRegistry();
driverRegistry.Register(new AccessDbDriver());
driverRegistry.Register(new SqliteDriver());

var commandRegistry = new CommandRegistry();
commandRegistry.Register(new HelpCommand(commandRegistry));
commandRegistry.Register(new DriversCommand(driverRegistry));
commandRegistry.Register(new ConnectCommand(driverRegistry));
commandRegistry.Register(new DisconnectCommand());
commandRegistry.Register(new TablesCommand());
commandRegistry.Register(new QueryCommand());
commandRegistry.Register(new ExportCommand(driverRegistry));
commandRegistry.Register(new SchemaCommand());
commandRegistry.Register(new ClearCommand());

if (args.Contains("--headless"))
{
    var session = new CliSession();
    var headlessArgs = args.Where(a => a != "--headless").ToArray();
    if (!headlessArgs.Any()) return;

    var safeArgs = headlessArgs
        .Select(a => a.Contains(' ') ? $"\"{a}\"" : a)
        .ToArray();

    var input = string.Join(" ", safeArgs);
    var (commandName, context) = CommandParser.Parse(input);

    if (!context.Options.ContainsKey("driver"))
    {
        AnsiConsole.MarkupLine("[red]No driver specified. Specify it with --driver[/]");
        return;
    }
    if (!context.Options.ContainsKey("connection"))
    {
        AnsiConsole.MarkupLine("[red]No connection string specified.[/]");
        return;
    }

// Initialize driver and session
    var driver = driverRegistry.GetDriver(context.Options["driver"]);
    var options = driver.BuildOptionsFromConnectionString(context.Options["connection"]);
    var connection = await driver.OpenConnectionAsync(options);
    session.Driver = driver;
    session.Connection = connection.Data;
    session.Options = options;
    session.Registry = driverRegistry;

// Ensure a command is provided
    if (string.IsNullOrWhiteSpace(commandName))
    {
        AnsiConsole.MarkupLine("[red]No command specified.[/]");
        return;
    }

// Execute the command
    if (!commandRegistry.TryGet(commandName, out var command))
    {
        AnsiConsole.MarkupLine($"[red]Unknown command: {commandName}[/]");
        return;
    }

    try
    {
        await command.Execute(context, session);
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]Error executing command:[/] {ex.Message}");
    }
}
else
{
    var cli = new InteractiveCli(commandRegistry);
    await cli.Run();
}