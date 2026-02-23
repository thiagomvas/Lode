using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Core.Errors;
using Lode.Core.ValueTypes;

namespace Lode.Drivers.AccessDb;

public sealed class AccessDbDriver : IDbDriver
{
    public string Name => "AccessDb";
    public DriverCapabilities Capabilities =>
        DriverCapabilities.Read;

    public async Task<Result<IDbConnection>> OpenConnectionAsync(DbConnectionOptions options, CancellationToken cancellationToken = default)
    {
        var isMdbAvailable = await ProcessRunner.IsMdbToolsAvailable();
        if (!isMdbAvailable)
            return DriverErrors.MissingDependency("mdb-tools is not installed and is required to read Access DB files");

        if (string.IsNullOrWhiteSpace(options.FilePath))
            return DriverErrors.ConnectionFailed("FilePath is missing");
        
        var connection = new AccessDbConnection(options.FilePath) { FormattedName = $"accdb@{Path.GetFileName(options.FilePath)}"};
        
        return Result<IDbConnection>.Success(connection);
    }

    public string BuildConnectionString(DbConnectionOptions options)
    {
        return options.FilePath;
    }

    public DbConnectionOptions GetDefaultOptions()
    {
        return new();
    }

    public DbConnectionOptions BuildOptionsFromConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        return new DbConnectionOptions
        {
            FilePath = connectionString.Trim(),
            Options = new Dictionary<string, string>()
        };
    }
}
