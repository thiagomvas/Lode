using Lode.Core.Models;
using Lode.Core.Models.Schema;

namespace Lode.Core.Abstractions;

public interface IImporter
{
    Task<Result> ImportAsync(
        IAsyncEnumerable<CanonicalRow> rows,
        TableDefinition table,
        CancellationToken cancellationToken = default);
}