namespace Lode.Core.ValueTypes;

[Flags]
public enum DriverCapabilities
{
    None            = 0,
    Read            = 1 << 0,
    Write           = 1 << 1,
    Query         = 1 << 2,
    Schema          = 1 << 3,
    Transactions    = 1 << 4,
}