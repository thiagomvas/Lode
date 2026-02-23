using Lode.Cli;
using Spectre.Console;

public sealed class CommandRegistry
{
    private readonly Dictionary<string, ICliCommand> _commands =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(ICliCommand command)
    {
        _commands[command.Name] = command;
    }

    public bool TryGet(string name, out ICliCommand command)
    {
        return _commands.TryGetValue(name, out command);
    }

    public IEnumerable<ICliCommand> GetCommands() => _commands.Values;
}