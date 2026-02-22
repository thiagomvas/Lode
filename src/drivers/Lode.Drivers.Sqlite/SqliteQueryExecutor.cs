using Lode.Core;
using Lode.Core.Abstractions;
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
            var reader =  await command.ExecuteReaderAsync(cancellationToken);

            var columns = Enumerable.Range(0, reader.FieldCount)
                .Select(i => new ColumnDefinition(
                    reader.GetName(i),
                    SqliteUtils.MapToCanonical(reader.GetDataTypeName(i))
                ))
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
}
