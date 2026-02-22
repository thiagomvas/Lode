namespace Lode.Core.Abstractions;

public interface IDbConnection : IAsyncDisposable
{
    Task<Result> PingAsync(CancellationToken cancellationToken = default);
}