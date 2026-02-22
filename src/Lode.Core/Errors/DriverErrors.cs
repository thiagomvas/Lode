namespace Lode.Core.Errors;

public static class DriverErrors
{
    public static Error ConnectionFailed(string message) 
        => new Error("Driver.ConnectionFailed", message);
}
