using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Core.Models.Schema;

namespace Lode.Drivers.AccessDb;

public sealed class AccessDbSchemaProvider : ISchemaProvider
{
    private readonly string _file;
    private readonly IAccessDatabaseProvider _provider;


    public AccessDbSchemaProvider(string file, IAccessDatabaseProvider provider)
    {
        _file = file;
        _provider = provider;
    }

    public async Task<Result<IEnumerable<string>>> GetTableNamesAsync()
    {
        var names = await _provider.GetTablesAsync(_file);
        return Result<IEnumerable<string>>.Success(names);
    }

    public async Task<Result<TableDefinition>> GetTableDefinitionAsync(string tableName)
    {
        var schema = await _provider.GetTableSchemaAsync(_file, tableName);
        return Result<TableDefinition>.Success(schema);
    }

    public async Task<Result<string>> GetSchemaAsync()
    {
        var schema = await _provider.GetFullSchemaAsync(_file);
        return Result<string>.Success(schema);
    }
}
