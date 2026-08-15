namespace Nexus1.BuildingBlocks.Application;

/// <summary>
/// Refusals are first-class, not exceptions — Blueprint_to_Core's explicit
/// dissent from strict CQS (ADR-002-amend).
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error is not null)
        {
            throw new InvalidOperationException("A successful result cannot have an error.");
        }

        if (!isSuccess && error is null)
        {
            throw new InvalidOperationException("A failed result must have an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? Error { get; }

    public static Result Success() => new(true, null);

    public static Result Failure(string error) => new(false, error);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue? value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static Result<TValue> Success(TValue value) => new(value, true, null);

    public static new Result<TValue> Failure(string error) => new(default, false, error);
}
