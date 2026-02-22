namespace Lode.Core.Errors;

public static class SchemaErrors
{
    public static Error SchemaFailed(string message)
        => new Error("Schema.SchemaFailed", message);
    
    public static Error TableNotFound(string message)
        => new Error("Schema.TableNotFound", message);
    
    public static Error IntrospectionFailed(string message)
        => new Error("Schema.IntrospectionFailed", message);
}