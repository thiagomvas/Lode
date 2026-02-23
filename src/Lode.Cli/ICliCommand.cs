namespace Lode.Cli;

using Lode.Cli;

public interface ICliCommand
{
    string Name { get; }
    Task Execute(CommandContext context, CliSession session);
}