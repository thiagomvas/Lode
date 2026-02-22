namespace Lode.Core.Errors;

public static class SchemaErrors
{
    public static Error SchemaFailed(string message)
        => new Error("SchemaFailed", message);
}
