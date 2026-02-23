using System.Collections.Generic;
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
            Args = new List<string>(),
            Options = new Dictionary<string, string>()
        };

        for (int i = 1; i < args.Count; i++)
        {
            var arg = args[i];

            if (arg.StartsWith("-") && arg.Contains("="))
            {
                // only treat as option if it starts with - or --
                var parts = arg.TrimStart('-').Split('=', 2);
                context.Options[parts[0]] = parts[1].Trim('"');
            }
            else if (arg.StartsWith("-"))
            {
                var key = arg.TrimStart('-');
                string value = null;

                if (i + 1 < args.Count && !args[i + 1].StartsWith("-"))
                {
                    value = args[i + 1].Trim('"');
                    i++;
                }

                context.Options[key] = value ?? "true";
            }
            else
            {
                // everything else is positional argument
                context.Args.Add(arg);
            }
        }

        return (command, context);
    }

    private static IEnumerable<string> SplitArgs(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            yield break;

        var current = new System.Text.StringBuilder();
        bool inQuotes = false;
        char quoteChar = '\0';

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (inQuotes)
            {
                if (c == quoteChar)
                {
                    inQuotes = false; // closing quote
                }
                else if (c == '\\' && i + 1 < input.Length && input[i + 1] == quoteChar)
                {
                    current.Append(quoteChar); // escaped quote
                    i++;
                }
                else
                {
                    current.Append(c); // keep everything inside quotes
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