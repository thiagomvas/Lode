using Lode.Core.Models.Schema;

namespace Lode.Core.Models;

public sealed class QueryResult
{
    public required IReadOnlyList<ColumnDefinition> Columns { get; init; }
    public required IReadOnlyList<IReadOnlyList<object?>> Rows { get; init; }
    public required int TotalRows { get; init; }
    public required int PageSize { get; init; }
    public required int PageNumber { get; init; }
    public required int PageCount { get; init; }
}
