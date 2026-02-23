namespace Lode.Cli;

public sealed class CommandContext
{
    public Dictionary<string, string> Options { get; init; } = new();
    public List<string> Args { get; init; } = new();
}