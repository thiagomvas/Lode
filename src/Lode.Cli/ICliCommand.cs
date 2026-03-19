namespace Lode.Cli;

public interface ICliCommand
{
    string Name { get; }
    string Description { get; }
    string Usage { get; }
    bool RequiresConnection { get; }
    Task Execute(CommandContext context, CliSession session);
}