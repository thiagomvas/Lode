namespace Lode.Core;

public sealed class Result<T> where T : notnull
{
    public T Data { get; init; }
    public IEnumerable<Error> Errors { get; init; }
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;

    protected Result(T data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Data = data;
        Errors = [];
        IsSuccess = true;
    }

    protected Result(IEnumerable<Error> errors)
    {
        Errors = errors;
        IsSuccess = false;
    }

    public static implicit operator Result<T>(Error error) => new([error]);
    public static implicit operator T (Result<T> result) => result.Data!;

    public static Result<T> Success(T data) => new(data);
    public static Result<T> Failure(params IEnumerable<Error> errors) => new(errors);
}
