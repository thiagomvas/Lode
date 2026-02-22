using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Core.Errors;
using Microsoft.Data.Sqlite;

namespace Lode.Drivers.Sqlite;

public sealed class SqliteDriver : IDbDriver
{
    public async Task<Result<IDbConnection>> OpenConnectionAsync(DbConnectionOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var connString = BuildConnectionString(options);
            var connection = new SqliteConnection(connString);
            await connection.OpenAsync(cancellationToken);

            return Result<IDbConnection>.Success(new SqliteDbConnection(connection));
        }
        catch (Exception ex)
        {
            return DriverErrors.ConnectionFailed(ex.Message);
        }
    }

    public string BuildConnectionString(DbConnectionOptions options)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = options.FilePath
        };
        
        foreach (var (key, value) in options.Options)
        {
            switch (key.ToUpperInvariant())
            {
                case "MODE":
                    if (Enum.TryParse<SqliteOpenMode>(value, ignoreCase: true, out var mode))
                        builder.Mode = mode;
                    break;

                case "CACHE":
                    if (Enum.TryParse<SqliteCacheMode>(value, ignoreCase: true, out var cache))
                        builder.Cache = cache;
                    break;

                case "PASSWORD":
                    builder.Password = value;
                    break;

                case "DEFAULTTIMEOUT":
                    if (int.TryParse(value, out var timeout))
                        builder.DefaultTimeout = timeout;
                    break;

                case "POOLING":
                    if (bool.TryParse(value, out var pooling))
                        builder.Pooling = pooling;
                    break;

                case "FOREIGNKEYS":
                    if (bool.TryParse(value, out var foreignKeys))
                        builder.ForeignKeys = foreignKeys;
                    break;

                case "RECURSIVTRIGGERS":
                    if (bool.TryParse(value, out var recursiveTriggers))
                        builder.RecursiveTriggers = recursiveTriggers;
                    break;
            }
        }

        return builder.ToString();
    }

    public DbConnectionOptions GetDefaultOptions() => new()
    {
        FilePath = ":memory:",
        Options =
        {
            ["Mode"] = "Memory",
            ["Cache"] = "Shared",
            ["Pooling"] = "false",
            ["DefaultTimeout"] = "30",
            ["ForeignKeys"] = "true"
        }
    };
}
