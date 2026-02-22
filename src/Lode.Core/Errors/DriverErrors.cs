namespace Lode.Core.Errors;

public static class DriverErrors
{
    public static Error ConnectionFailed(string message) 
        => new Error("Driver.ConnectionFailed", message);
    
    public static Error NotSupported(string message)
        => new Error("Driver.NotSupported", message);
    
    public static Error MissingDependency(string message)
        => new Error("Driver.MissingDependency", message);
}
