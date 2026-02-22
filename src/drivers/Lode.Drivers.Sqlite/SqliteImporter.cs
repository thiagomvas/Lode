using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Core.Models;
using Lode.Core.Models.Schema;
using Lode.Core.ValueTypes;
using Microsoft.Data.Sqlite;

namespace Lode.Drivers.Sqlite;

public sealed class SqliteImporter : IImporter
{
    private readonly SqliteConnection _connection;

    public SqliteImporter(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async Task<Result> ImportAsync(
        IAsyncEnumerable<CanonicalRow> rows,
        TableDefinition table,
        CancellationToken cancellationToken = default)
    {
        var columns = table.Columns.OrderBy(x => x.Id).ToArray();

        var createSql =
            $"CREATE TABLE IF NOT EXISTS {SqliteUtils.EscapeIdentifier(table.Name)} (" +
            string.Join(", ", columns.Select(BuildColumn)) +
            ")";

        var createCommand = _connection.CreateCommand();
        createCommand.CommandText = createSql;
        await createCommand.ExecuteNonQueryAsync(cancellationToken);

        var columnNames = string.Join(", ", columns.Select(c => SqliteUtils.EscapeIdentifier(c.Name)));
        var paramNames = string.Join(", ", columns.Select((_, i) => $"@p{i}"));

        var insertCommand = _connection.CreateCommand();
        insertCommand.CommandText =
            $"INSERT INTO {SqliteUtils.EscapeIdentifier(table.Name)} ({columnNames}) VALUES ({paramNames})";

        for (var i = 0; i < columns.Length; i++)
            insertCommand.Parameters.Add(new SqliteParameter($"@p{i}", null));

        await using var tx = await _connection.BeginTransactionAsync(cancellationToken);
        insertCommand.Transaction = (Microsoft.Data.Sqlite.SqliteTransaction) tx;

        await foreach (var row in rows.WithCancellation(cancellationToken))
        {
            for (var i = 0; i < row.Values.Count; i++)
                insertCommand.Parameters[i].Value = row.Values[i] ?? DBNull.Value;

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);

        foreach (var column in columns)
        {
            if (column.Flags.HasFlag(ColumnFlags.Indexed))
            {
                var indexCommand = _connection.CreateCommand();
                indexCommand.CommandText =
                    $"CREATE INDEX IF NOT EXISTS {SqliteUtils.EscapeIdentifier($"idx_{table.Name}_{column.Name}")} " +
                    $"ON {SqliteUtils.EscapeIdentifier(table.Name)} ({SqliteUtils.EscapeIdentifier(column.Name)})";

                await indexCommand.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        return Result.Success();
    }

    private static string BuildColumn(ColumnDefinition column)
    {
        if (column.Type == CanonicalType.Unknown)
            throw new InvalidOperationException($"Unknown type for column {column.Name}");

        var parts = new List<string>();

        parts.Add(SqliteUtils.EscapeIdentifier(column.Name));
        parts.Add(SqliteUtils.MapFromCanonical(column.Type));

        if (column.Flags.HasFlag(ColumnFlags.PrimaryKey))
            parts.Add("PRIMARY KEY");

        if (column.Flags.HasFlag(ColumnFlags.AutoIncrement))
            parts.Add("AUTOINCREMENT");

        if (column.Flags.HasFlag(ColumnFlags.UniqueKey))
            parts.Add("UNIQUE");

        if (column.Flags.HasFlag(ColumnFlags.NotNull))
            parts.Add("NOT NULL");

        if (column.Flags.HasFlag(ColumnFlags.Default) && column.DefaultValue != null)
            parts.Add($"DEFAULT {SqliteUtils.FormatLiteral(column.DefaultValue)}");

        return string.Join(" ", parts);
    }
}