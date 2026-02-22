using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Lode.Core.Models;
using Lode.Core.Models.Schema;
using Lode.Core.ValueTypes;

namespace Lode.Drivers.AccessDb;
public sealed class LinuxMdbToolsProvider : IAccessDatabaseProvider
{
    public async Task<IEnumerable<string>> GetTablesAsync(string file)
    {
        var result = await ProcessRunner.RunAsync("mdb-tables", $"-1 \"{file}\"");
        return result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    }

    public async Task<TableDefinition> GetTableSchemaAsync(string file, string table)
    {
        var result = await ProcessRunner.RunAsync("mdb-schema", $"\"{file}\" sqlite --not-null --default-values --not-empty");
        
        var tableSchema = ExtractTable(result.StdOut.Trim(), table);
        
        var columns = AccessSchemaParser.ParseColumns(tableSchema);

        var def = new TableDefinition()
        {
            Name = table,
            Columns = columns
        };
        return def;
    }

    public async IAsyncEnumerable<CanonicalRow> ExportTableAsync(string file, string table)
    {
        var schema = await GetTableSchemaAsync(file, table);
        var columns = schema.Columns.OrderBy(x => x.Id).ToArray();
        var delimiter = '\x1F';

        var psi = new ProcessStartInfo
        {
            FileName = "mdb-export",
            Arguments = $"-H -d \"{delimiter}\" \"{file}\" \"{table}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);

        using var reader = process.StandardOutput;

        var header = await reader.ReadLineAsync();

        while (true)
        {
            var line = await reader.ReadLineAsync();

            if (line == null)
                break;

            var parts = ParseDelimited(line, '\x1F');
            
            var values = new object?[columns.Length];

            for (var i = 0; i < columns.Length; i++)
            {
                var raw = i < parts.Count ? parts[i] : null;
                values[i] = ConvertValue(raw, columns[i].Type);
            }

            yield return new CanonicalRow
            {
                Values = values
            };
        }

        await process.WaitForExitAsync();
    }

    public async Task<string> GetFullSchemaAsync(string file)
    {
        var result = await ProcessRunner.RunAsync("mdb-schema", $"\"{file}\" sqlite --not-null --default-values --not-empty");
        return result.StdOut.Trim();
    }

    private static List<string?> ParseDelimited(string line, char delimiter)
    {
        var result = new List<string?>();
        var sb = new System.Text.StringBuilder();

        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (c == delimiter && !inQuotes)
            {
                result.Add(sb.Length == 0 ? null : sb.ToString());
                sb.Clear();
                continue;
            }

            sb.Append(c);
        }

        result.Add(sb.Length == 0 ? null : sb.ToString());

        return result;
    }
    private static object? ConvertValue(string? value, CanonicalType type)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        switch (type)
        {
            case CanonicalType.SmallInt:
                return short.Parse(value);

            case CanonicalType.Int:
                if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var i))
                    return i;
                return null;

            case CanonicalType.BigInt:
                return long.Parse(value);

            case CanonicalType.Float:
                return float.Parse(value);

            case CanonicalType.Double:
                return double.Parse(value);

            case CanonicalType.Decimal:
                return decimal.Parse(value);

            case CanonicalType.Boolean:
                return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);

            case CanonicalType.Guid:
                return Guid.Parse(value);

            case CanonicalType.Date:
            case CanonicalType.Time:
            case CanonicalType.DateTime:
            case CanonicalType.DateTimeOffset:
                return DateTime.Parse(value);

            case CanonicalType.Blob:
                return Convert.FromBase64String(value);

            case CanonicalType.String:
            case CanonicalType.Char:
            case CanonicalType.Json:
            case CanonicalType.Xml:
                return value;

            default:
                return value;
        }
    }
    private static string ExtractTable(string schema, string tableName)
    {
        var pattern = $@"CREATE\s+TABLE\s+`{Regex.Escape(tableName)}`[\s\S]*?\);";
        var match = Regex.Match(schema, pattern, RegexOptions.IgnoreCase);

        return match.Success ? match.Value : null;
    }
}