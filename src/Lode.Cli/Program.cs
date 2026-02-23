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

var cli = new InteractiveCli(commandRegistry);
await cli.Run();