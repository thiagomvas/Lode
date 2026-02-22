using Lode.Core.Models;
using Lode.Core.Models.Schema;

namespace Lode.Drivers.AccessDb;

public interface IAccessDatabaseProvider
{
    Task<IEnumerable<string>> GetTablesAsync(string file);

    Task<TableDefinition> GetTableSchemaAsync(string file, string table);

    IAsyncEnumerable<CanonicalRow> ExportTableAsync(string file, string table);
}