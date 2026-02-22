using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Core.Errors;
using Lode.Core.Models.Schema;
using Lode.Core.ValueTypes;
using Microsoft.Data.Sqlite;

namespace Lode.Drivers.Sqlite;

public sealed class SqliteSchemaProvider : ISchemaProvider
{
    private readonly SqliteConnection _connection;

    public SqliteSchemaProvider(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async Task<Result<IEnumerable<string>>> GetTableNamesAsync()
    {
        try
        {
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name";
            var reader = await command.ExecuteReaderAsync();
            var names = new List<string>();
            while (await reader.ReadAsync())
            {
                names.Add(reader.GetString(0));
            }

            return Result<IEnumerable<string>>.Success(names);
        }
        catch (Exception ex)
        {
            return SchemaErrors.IntrospectionFailed(ex.Message);
        }
    }

    public async Task<Result<TableDefinition>> GetTableDefinitionAsync(string tableName)
    {
        try
        {
            var command = _connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info({tableName})";
            var reader = await command.ExecuteReaderAsync();

            List<ColumnDefinition> columns = new();

            while (await reader.ReadAsync())
            {
                var id = reader.GetInt32(0);
                var name = reader.GetString(1);
                var type = reader.GetString(2);
                var notNull = reader.GetBoolean(3);
                var defaultValue = reader.IsDBNull(4) ? null : reader.GetValue(4);
                var pk = reader.GetBoolean(5);

                ColumnFlags flags = ColumnFlags.None;
                if (pk) flags |= ColumnFlags.PrimaryKey;
                if (notNull) flags |= ColumnFlags.NotNull;
                else flags |= ColumnFlags.Nullable;

                var column = new ColumnDefinition()
                {
                    Id = id,
                    Name = name,
                    DefaultValue = defaultValue,
                    Type = SqliteUtils.MapToCanonical(type),
                    Flags = flags
                };

                columns.Add(column);
            }

            var table = new TableDefinition()
            {
                Name = tableName,
                Columns = columns
            };
            
            return Result<TableDefinition>.Success(table);
        }
        catch (Exception ex)
        {
            return SchemaErrors.IntrospectionFailed(ex.Message);
        }
    }

    public async Task<Result<string>> GetSchemaAsync()
    {
        try
        {
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT name, sql FROM sqlite_master WHERE type = 'table' ORDER BY name;";
            var reader = await command.ExecuteReaderAsync();

            var builder = new System.Text.StringBuilder();

            while (await reader.ReadAsync())
            {
                var name = reader.GetString(0);
                var sql = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                builder.AppendLine(sql);
                builder.AppendLine();
            }

            return Result<string>.Success(builder.ToString().Trim());
        }
        catch (Exception ex)
        {
            return SchemaErrors.SchemaFailed(ex.Message);
        }
    }
}