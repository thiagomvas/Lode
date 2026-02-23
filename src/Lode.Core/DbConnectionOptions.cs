namespace Lode.Core;

public sealed class DbConnectionOptions
{
    #region File Based
    
    public string? FilePath { get; set; }
    
    #endregion
    
    public string? Host { get; set; }
    public int? Port { get; set; } 
    public string? Database { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    
    public Dictionary<string, string> Options { get; init; } = [];
}
