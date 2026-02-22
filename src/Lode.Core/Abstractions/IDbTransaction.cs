namespace Lode.Core.Abstractions;

public interface IDbTransaction : IAsyncDisposable
{
    Task<Result> CommitAsync(CancellationToken cancellationToken = default);
    Task<Result> RollbackAsync(CancellationToken cancellationToken = default);
}