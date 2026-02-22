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

            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys = ON;";
            await command.ExecuteNonQueryAsync(cancellationToken);

            return Result<IDbConnection>.Success(new SqliteDbConnection(connection));
        }
        catch (Exception ex)
        {
            return DriverErrors.ConnectionFailed(ex.Message);
        }
    }

    public string BuildConnectionString(DbConnectionOptions options)
    {
        var builder = new SqliteConnectionStringBuilder()
        {
            DataSource = options.FilePath
        };
        
        return builder.ToString();
    }
}
