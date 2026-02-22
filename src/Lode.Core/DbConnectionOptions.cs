namespace Lode.Core;

public sealed class DbConnectionOptions
{
    #region File Based
    
    public string? FilePath { get; set; }
    
    #endregion
    
    public Dictionary<string, string> Options { get; init; } = [];
}
