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
}