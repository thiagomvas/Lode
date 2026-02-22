namespace Lode.Core.Abstractions;

public interface IMigrationService
{
    Task<Result> MigrateAsync(
        IDbConnection source,
        IDbConnection destination,
        IEnumerable<string>? tables = null,
        CancellationToken cancellationToken = default);
}