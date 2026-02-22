using System.Text.RegularExpressions;
using Lode.Core.Models.Schema;
using Lode.Core.ValueTypes;

namespace Lode.Drivers.AccessDb;
public static class AccessSchemaParser
{
    private static readonly Regex ColumnRegex =
        new(@"`(?<name>[^`]+)`\s+(?<type>\w+)(?:\s+DEFAULT\s+(?<default>[^,\n]+))?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static ColumnDefinition[] ParseColumns(string createTableSql)
    {
        var start = createTableSql.IndexOf('(');
        var end = createTableSql.LastIndexOf(')');
        var body = createTableSql.Substring(start + 1, end - start - 1);

        var lines = body
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var columns = new List<ColumnDefinition>();
        var id = 0;

        foreach (var line in lines)
        {
            var cleaned = line.Trim().TrimEnd(',');

            var match = ColumnRegex.Match(cleaned);
            if (!match.Success)
                continue;

            var name = match.Groups["name"].Value;
            var type = match.Groups["type"].Value;
            var defaultValue = match.Groups["default"].Success
                ? ParseDefault(match.Groups["default"].Value)
                : null;

            var flags = ColumnFlags.None;

            if (defaultValue != null)
                flags |= ColumnFlags.Default;

            columns.Add(new ColumnDefinition
            {
                Id = id++,
                Name = name,
                Type = MapType(type),
                DefaultValue = defaultValue,
                Flags = flags
            });
        }

        return columns.ToArray();
    }

    private static CanonicalType MapType(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "integer" => CanonicalType.Int,
            "int" => CanonicalType.Int,
            "bigint" => CanonicalType.BigInt,
            "smallint" => CanonicalType.SmallInt,
            "float" => CanonicalType.Float,
            "double" => CanonicalType.Double,
            "decimal" => CanonicalType.Decimal,
            "varchar" => CanonicalType.String,
            "text" => CanonicalType.String,
            "char" => CanonicalType.Char,
            "date" => CanonicalType.Date,
            "time" => CanonicalType.Time,
            "datetime" => CanonicalType.DateTime,
            "blob" => CanonicalType.Blob,
            "boolean" => CanonicalType.Boolean,
            _ => CanonicalType.Unknown
        };
    }

    private static object ParseDefault(string value)
    {
        value = value.Trim();

        if (int.TryParse(value, out var i))
            return i;

        if (double.TryParse(value, out var d))
            return d;

        if (value.StartsWith("'") && value.EndsWith("'"))
            return value[1..^1];

        return value;
    }
}