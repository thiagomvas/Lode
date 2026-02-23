using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Lode.Cli;

public static class CommandParser
{
    public static (string Command, CommandContext Context) Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return (null, null);

        var args = SplitArgs(input).ToList();
        if (!args.Any()) return (null, null);

        var command = args[0];
        var context = new CommandContext
        {
            Args = args.Skip(1).ToList()
        };

        return (command, context);
    }

    private static IEnumerable<string> SplitArgs(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            yield break;

        bool inQuotes = false;
        char quoteChar = '\0';
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (inQuotes)
            {
                if (c == quoteChar)
                {
                    inQuotes = false;
                }
                else if (c == '\\' && i + 1 < input.Length && input[i + 1] == quoteChar)
                {
                    // Handle escaped quote inside quotes
                    current.Append(quoteChar);
                    i++; // Skip the escaped quote
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"' || c == '\'')
                {
                    inQuotes = true;
                    quoteChar = c;
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (current.Length > 0)
                    {
                        yield return current.ToString();
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        if (current.Length > 0)
            yield return current.ToString();
    }
}