using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Core.Errors;
using Lode.Core.Models.Schema;

namespace Lode.Business;

public sealed class MigrationService : IMigrationService
{
    public async Task<Result> MigrateAsync(
        IDbConnection source,
        IDbConnection destination,
        IEnumerable<string>? tables = null,
        CancellationToken cancellationToken = default)
    {
        var tableNamesResult = await source.Schema.GetTableNamesAsync();
        if (!tableNamesResult.IsSuccess)
            return Result.Failure(tableNamesResult.Errors);

        var sourceTables = tableNamesResult.Data.ToHashSet(StringComparer.OrdinalIgnoreCase);

        IEnumerable<string> tablesToMigrate;

        var tablesArr = tables as string[] ?? tables.ToArray();
        if (tables is null || !tablesArr.Any())
        {
            tablesToMigrate = sourceTables;
        }
        else
        {
            var requested = tablesArr.ToList();

            foreach (var table in requested)
            {
                if (!sourceTables.Contains(table))
                    return SchemaErrors.TableNotFound($"Table '{table}' does not exist in source database.");
            }

            tablesToMigrate = requested;
        }

        foreach (var tableName in tablesToMigrate)
        {
            var schemaResult = await source.Schema.GetTableDefinitionAsync(tableName);
            if (!schemaResult.IsSuccess)
                return Result.Failure(schemaResult.Errors);

            TableDefinition schema = schemaResult.Data;

            var rows = source.Exporter.ExportAsync(tableName, cancellationToken);

            await destination.Importer.ImportAsync(rows, schema, cancellationToken);
        }

        return Result.Success();
    }
}