using Lode.Core.ValueTypes;

namespace Lode.Core.Models.Schema;

public sealed record ColumnDefinition(string Name, CanonicalType Type);
