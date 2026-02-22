namespace Lode.Core.Models.Schema;

public sealed class TableDefinition
{
    public required string Name { get; set; }
    public required IEnumerable<ColumnDefinition> Columns { get; set; }
}
