namespace Lode.Core.Abstractions;

public interface IDbDriver
{
    Task<Result<IDbConnection>> OpenConnectionAsync(DbConnectionOptions options, CancellationToken cancellationToken = default);
    string BuildConnectionString(DbConnectionOptions options);
    DbConnectionOptions GetDefaultOptions();
}