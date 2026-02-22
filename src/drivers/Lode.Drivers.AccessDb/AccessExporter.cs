using Lode.Core.Abstractions;
using Lode.Core.Models;

namespace Lode.Drivers.AccessDb;

public sealed class AccessExporter : IExporter
{
    private readonly string _file;
    private readonly IAccessDatabaseProvider _provider;

    public AccessExporter(string file, IAccessDatabaseProvider provider)
    {
        _file = file;
        _provider = provider;
    }

    public IAsyncEnumerable<CanonicalRow> ExportAsync(string tableName, CancellationToken cancellationToken = default)
    {
        return _provider.ExportTableAsync(_file, tableName);
    }
}
