using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Core.Errors;
using Microsoft.Data.Sqlite;

namespace Lode.Drivers.Sqlite;

public sealed class SqliteDbConnection : IDbConnection
{
    private readonly SqliteConnection _connection;

    public SqliteDbConnection(SqliteConnection connection)
    {
        _connection = connection;
        Query = new SqliteQueryExecutor(connection);
        Schema = new SqliteSchemaProvider(connection);
        Exporter = new SqliteExporter(connection);
        Importer = new SqliteImporter(connection);
    }

    public ISchemaProvider Schema { get; }
    public IQueryExecutor Query { get; }
    public IImporter Importer { get; }
    public IExporter Exporter { get; }

    public async Task<Result> PingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = _connection.CreateCommand();
            command.CommandText = "SELECT 1;";
            await command.ExecuteScalarAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return DriverErrors.ConnectionFailed(ex.Message);
        }
    }

    public async Task<Result<IDbTransaction>> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var transaction = await _connection.BeginTransactionAsync(cancellationToken);
            return Result<IDbTransaction>.Success(new SqliteTransaction((Microsoft.Data.Sqlite.SqliteTransaction) transaction));
        }
        catch (Exception ex)
        {
            return TransactionErrors.TransactionFailed(ex.Message);
        }
    }


    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
