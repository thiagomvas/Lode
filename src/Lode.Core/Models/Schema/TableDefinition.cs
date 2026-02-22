namespace Lode.Core.Models.Schema;

public sealed class TableDefinition
{
    public string Name { get; set; }
    public IEnumerable<ColumnDefinition> Columns { get; set; }
}
