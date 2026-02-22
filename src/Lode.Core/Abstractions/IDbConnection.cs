namespace Lode.Core.Abstractions;

public interface IDbConnection : IAsyncDisposable
{
    IQueryExecutor Query { get; }
    Task<Result> PingAsync(CancellationToken cancellationToken = default);
    Task<Result<IDbTransaction>> BeginTransactionAsync(CancellationToken cancellationToken = default);
}