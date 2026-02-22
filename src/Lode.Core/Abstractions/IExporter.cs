using Lode.Core.Models;

namespace Lode.Core.Abstractions;

public interface IExporter
{
    IAsyncEnumerable<CanonicalRow> ExportAsync(string tableName, CancellationToken cancellationToken = default);
}