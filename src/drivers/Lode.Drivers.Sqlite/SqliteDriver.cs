using System.Data.Common;
using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Core.Errors;
using Lode.Core.ValueTypes;
using Microsoft.Data.Sqlite;

namespace Lode.Drivers.Sqlite;

public sealed class SqliteDriver : IDbDriver
{
    public string Name => "Sqlite";

    public DriverCapabilities Capabilities =>
        DriverCapabilities.Read |
        DriverCapabilities.Write |
        DriverCapabilities.Schema |
        DriverCapabilities.Transactions;

    public async Task<Result<IDbConnection>> OpenConnectionAsync(DbConnectionOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connString = BuildConnectionString(options);
            var connection = new SqliteConnection(connString);
            await connection.OpenAsync(cancellationToken);

            return Result<IDbConnection>.Success(new SqliteDbConnection(connection) { FormattedName = $"sqlite@{Path.GetFileName(options.FilePath)}"});
        }
        catch (Exception ex)
        {
            return DriverErrors.ConnectionFailed(ex.Message);
        }
    }

    public string BuildConnectionString(DbConnectionOptions options)
    {
        // Use FilePath if provided; otherwise default to in-memory
        var dataSource = string.IsNullOrWhiteSpace(options.FilePath) ? ":memory:" : options.FilePath;

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = dataSource
        };

        foreach (var (key, value) in options.Options)
        {
            switch (key.ToUpperInvariant())
            {
                case "MODE":
                    if (!Enum.TryParse<SqliteOpenMode>(value, ignoreCase: true, out var parsedMode))
                        continue;

                    builder.Mode = parsedMode;
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
                case "RECURSIVETRIGGERS":
                    if (bool.TryParse(value, out var recursiveTriggers))
                        builder.RecursiveTriggers = recursiveTriggers;
                    break;
            }
        }

        if (!builder.ContainsKey("Mode") ||
            builder.Mode == SqliteOpenMode.Memory && !string.IsNullOrWhiteSpace(options.FilePath))
        {
            builder.Mode = string.IsNullOrWhiteSpace(options.FilePath)
                ? SqliteOpenMode.Memory
                : SqliteOpenMode.ReadWriteCreate;
        }

        return builder.ToString();
    }

    public DbConnectionOptions GetDefaultOptions() => new()
    {
        FilePath = null,
        Options =
        {
            ["Mode"] = "Memory",
            ["Cache"] = "Shared",
            ["Pooling"] = "false",
            ["DefaultTimeout"] = "30",
            ["ForeignKeys"] = "true"
        }
    };

    public DbConnectionOptions BuildOptionsFromConnectionString(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);

        var options = new DbConnectionOptions
        {
            FilePath = builder.DataSource == ":memory:" ? null : builder.DataSource
        };

        options.Options["Mode"] = builder.Mode.ToString();
        options.Options["Cache"] = builder.Cache.ToString();
        options.Options["Pooling"] = builder.Pooling.ToString();
        options.Options["Password"] = builder.Password ?? string.Empty;
        options.Options["DefaultTimeout"] = builder.DefaultTimeout.ToString();
        options.Options["ForeignKeys"] = builder.ForeignKeys.ToString();
        options.Options["RecursiveTriggers"] = builder.RecursiveTriggers.ToString();

        return options;
    }
}