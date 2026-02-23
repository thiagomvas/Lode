using Lode.Core.Models;

namespace Lode.Core.Abstractions;

public interface IQueryExecutor
{
    Task<Result<QueryResult>> ExecuteQueryAsync(string sql, CancellationToken cancellationToken = default);
    Task<Result<long>> ExecuteNonQueryAsync(string sql, CancellationToken cancellationToken = default);
    Task<Result<T>> ExecuteScalarAsync<T>(string sql, CancellationToken cancellationToken = default) where T : notnull;
    Task<Result<long>> InsertAsync(string table, IReadOnlyDictionary<string, IEnumerable<object?>> values, CancellationToken cancellationToken = default);
}