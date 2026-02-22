using Lode.Core.ValueTypes;

namespace Lode.Drivers.Sqlite;

public static class SqliteUtils
{
    public static CanonicalType MapToCanonical(string type) =>
        type.ToUpperInvariant().Trim() switch
        {
            "INT" or "INTEGER" or "INT4" or "INT8" or "MEDIUMINT" or "SIGNED" => CanonicalType.Int,
            "TINYINT" or "SMALLINT" or "INT2" => CanonicalType.SmallInt,
            "BIGINT" or "UNSIGNED BIG INT" => CanonicalType.BigInt,

            "REAL" or "FLOAT" => CanonicalType.Float,
            "DOUBLE" or "DOUBLE PRECISION" => CanonicalType.Double,
            "DECIMAL" or "NUMERIC" => CanonicalType.Decimal,

            "TEXT" or "CLOB" or "VARCHAR" or "VARYING CHARACTER" or
                "NCHAR" or "NATIVE CHARACTER" or "NVARCHAR" => CanonicalType.String,
            "CHAR" or "CHARACTER" => CanonicalType.Char,

            "BOOLEAN" => CanonicalType.Boolean,

            "DATE" => CanonicalType.Date,
            "TIME" => CanonicalType.Time,
            "DATETIME" or "TIMESTAMP" => CanonicalType.DateTime,

            "BLOB" or "BINARY" or "VARBINARY" => CanonicalType.Blob,

            "JSON" => CanonicalType.Json,
            "NULL" => CanonicalType.Null,

            _ => CanonicalType.Unknown
        };
    
    public static string MapFromCanonical(CanonicalType type) =>
        type switch
        {
            CanonicalType.SmallInt => "INTEGER",
            CanonicalType.Int => "INTEGER",
            CanonicalType.BigInt => "INTEGER",

            CanonicalType.Boolean => "INTEGER",

            CanonicalType.Float => "REAL",
            CanonicalType.Double => "REAL",
            CanonicalType.Decimal => "REAL",

            CanonicalType.String => "TEXT",
            CanonicalType.Char => "TEXT",

            CanonicalType.Date => "TEXT",
            CanonicalType.Time => "TEXT",
            CanonicalType.DateTime => "TEXT",
            CanonicalType.DateTimeOffset => "TEXT",

            CanonicalType.Guid => "TEXT",
            CanonicalType.Json => "TEXT",
            CanonicalType.Xml => "TEXT",

            CanonicalType.Blob => "BLOB",

            CanonicalType.Null => "NULL",

            _ => "TEXT"
        };
    
    public static string FormatLiteral(object value) =>
        value switch
        {
            null => "NULL",
            string s => $"'{s.Replace("'", "''")}'",
            bool b => b ? "1" : "0",
            DateTime dt => $"'{dt:O}'",
            Guid g => $"'{g}'",
            _ => value.ToString()
        };
    
    public static string EscapeIdentifier(string name)
    {
        return "\"" + name.Replace("\"", "\"\"") + "\"";
    }
}