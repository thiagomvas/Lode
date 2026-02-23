using System.Text.Json.Serialization;

namespace Lode.Core;

public sealed class Result
{
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<Error> Errors { get; init; } = [];

    protected Result()
    {
        IsSuccess = true;
        Errors = [];
    }
    
    protected Result(IEnumerable<Error> errors)
    {
        IsSuccess = false;
        Errors = errors;
    }
    
    public static implicit operator Result(Error error) => new([error]);
    public static Result Success() => new();
    
    public static Result Failure(params IEnumerable<Error> errors) => new(errors);
}
