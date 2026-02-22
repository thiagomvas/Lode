namespace Lode.Core.Models;

public sealed record CanonicalRow
{
    public required IReadOnlyList<object?> Values { get; init; }
}