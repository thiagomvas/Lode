using Lode.Core.Models.Schema;

namespace Lode.Core.Abstractions;

public interface ISchemaProvider
{
    Task<Result<IEnumerable<string>>> GetTableNamesAsync();
    Task<Result<TableDefinition>> GetTableDefinitionAsync(string tableName);
    Task<Result<string>> GetSchemaAsync();
}