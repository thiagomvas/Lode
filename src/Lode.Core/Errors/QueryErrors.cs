namespace Lode.Core.Errors;

public static class QueryErrors
{
    public static Error MissingArgument(string message)
        => new Error("Query.MissingArgument", message);
    public static Error InvalidArgument(string message)
        => new Error("Query.InvalidArgument", message);
}
