using Lode.Core.ValueTypes;

namespace Lode.Core.Models.Schema;

public sealed record ColumnDefinition()
{
    public int Id { get; init; }
    public string Name { get; init; }
    public CanonicalType Type { get; init; }
    public object DefaultValue { get; init; }
    public ColumnFlags Flags { get; init; }
}
