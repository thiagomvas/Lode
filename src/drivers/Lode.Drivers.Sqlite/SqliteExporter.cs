using Lode.Core.Abstractions;
using Lode.Core.Models;
using Microsoft.Data.Sqlite;

namespace Lode.Drivers.Sqlite;

public sealed class SqliteExporter : IExporter
{
    private readonly SqliteConnection _connection;

    public SqliteExporter(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async IAsyncEnumerable<CanonicalRow> ExportAsync(string tableName,
        CancellationToken cancellationToken = default)
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM " + tableName;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var fields = Enumerable.Range(0, reader.FieldCount)
                .Select(reader.GetValue)
                .ToList();
            yield return new CanonicalRow() { Values = fields };
        }
    }
}