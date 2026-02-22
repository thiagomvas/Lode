namespace Lode.Core.Abstractions;

public interface IDbConnection : IAsyncDisposable
{
    ISchemaProvider Schema { get; }
    IQueryExecutor Query { get; }
    IImporter Importer { get; }
    IExporter Exporter { get; }
    Task<Result> PingAsync(CancellationToken cancellationToken = default);
    Task<Result<IDbTransaction>> BeginTransactionAsync(CancellationToken cancellationToken = default);
}