namespace Lode.Core.ValueTypes;

[Flags]
public enum ColumnFlags
{
    None            = 0,
    PrimaryKey      = 1 << 0,
    ForeignKey      = 1 << 1,
    UniqueKey       = 1 << 2,
    NotNull         = 1 << 3,
    Nullable        = 1 << 4,
    AutoIncrement   = 1 << 5,
    Default         = 1 << 6,
    Indexed         = 1 << 7,
    Unsigned        = 1 << 8,
    Computed        = 1 << 9,
    Sparse          = 1 << 10,
    Identity        = 1 << 11,
}