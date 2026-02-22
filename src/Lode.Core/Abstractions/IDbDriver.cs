using Lode.Core.ValueTypes;

namespace Lode.Core.Abstractions;

public interface IDbDriver
{
    DriverCapabilities Capabilities { get; }
    Task<Result<IDbConnection>> OpenConnectionAsync(DbConnectionOptions options, CancellationToken cancellationToken = default);
    string BuildConnectionString(DbConnectionOptions options);
    DbConnectionOptions GetDefaultOptions();
}