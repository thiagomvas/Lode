namespace Lode.Core.Errors;

public static class TransactionErrors
{
    public static Error TransactionFailed(string message)
        => new Error("TransactionFailed", message);
}
