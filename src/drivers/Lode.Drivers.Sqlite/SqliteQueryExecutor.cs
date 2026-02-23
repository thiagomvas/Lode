using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Core.Errors;
using Lode.Core.Models;
using Lode.Core.Models.Schema;
using Microsoft.Data.Sqlite;

namespace Lode.Drivers.Sqlite;

public sealed class SqliteQueryExecutor : IQueryExecutor
{
    private readonly SqliteConnection _connection;

    public SqliteQueryExecutor(SqliteConnection connection)
    {
        _connection = connection;
    }


    public async Task<Result<QueryResult>> ExecuteQueryAsync(string sql, CancellationToken cancellationToken = default)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            await using var reader =  await command.ExecuteReaderAsync(cancellationToken);

            var columns = Enumerable.Range(0, reader.FieldCount)
                .Select(i => new ColumnDefinition()
                {
                    Name = reader.GetName(i),
                    Type = SqliteUtils.MapToCanonical(reader.GetDataTypeName(i))
                })
                .ToList();
            var rows = new List<IReadOnlyList<object?>>();

            while (await reader.ReadAsync(cancellationToken))
            {
                var row = Enumerable.Range(0, reader.FieldCount)
                    .Select(i => reader.IsDBNull(i) ? null : reader.GetValue(i))
                    .ToList();

                rows.Add(row);
            }
            
            var result = new QueryResult
            {
                Columns = columns,
                Rows = rows,
                TotalRows = rows.Count,
                PageSize = rows.Count,
                PageNumber = 1,
                PageCount = 1
            };

            return Result<QueryResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<QueryResult>.Failure();
        }
    }

    public async Task<Result<long>> ExecuteNonQueryAsync(string sql, CancellationToken cancellationToken = default)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            return Result<long>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<long>.Failure();
        }
    }

    public async Task<Result<T>> ExecuteScalarAsync<T>(string sql, CancellationToken cancellationToken = default) where T : notnull
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result is T t)
            {
                return Result<T>.Success(t);
            }
            return Result<T>.Failure();
            
        }
        catch (Exception ex)
        {
            return Result<T>.Failure();
        }
    }

    public async Task<Result<long>> InsertAsync(string table, IReadOnlyDictionary<string, IEnumerable<object?>> values, CancellationToken cancellationToken = default)
    {
        if (!values.Any()) 
            return QueryErrors.MissingArgument("No columns provided");

        // Convert all IEnumerable<object?> to arrays to allow indexing
        var columns = values.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());

        int rowCount = columns.Values.First().Length;
        if (columns.Values.Any(c => c.Length != rowCount))
            return QueryErrors.InvalidArgument("All columns must have the same number of values");

        var columnNames = string.Join(", ", columns.Keys.Select(SqliteUtils.EscapeIdentifier));
        long totalInserted = 0;

        await using var transaction = (Microsoft.Data.Sqlite.SqliteTransaction) await _connection.BeginTransactionAsync(cancellationToken);

        for (int i = 0; i < rowCount; i++)
        {
            var paramNames = string.Join(", ", columns.Keys.Select(c => $"@{c}{i}"));
            var sql = $"INSERT INTO {SqliteUtils.EscapeIdentifier(table)} ({columnNames}) VALUES ({paramNames})";

            await using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Transaction = transaction;

            foreach (var col in columns.Keys)
            {
                var value = columns[col][i];
                cmd.Parameters.AddWithValue($"@{col}{i}", value ?? DBNull.Value);
            }

            totalInserted += await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return Result<long>.Success(totalInserted);
    }

}
