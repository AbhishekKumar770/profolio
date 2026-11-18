using System.Diagnostics.CodeAnalysis;

namespace Profolio.Application.Common.Results;

public class Result
{
    private static readonly IReadOnlyList<Error> EmptyErrors = Array.AsReadOnly(Array.Empty<Error>());
    private static readonly Result SuccessInstance = new();

    private readonly IReadOnlyList<Error> _errors;

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public IReadOnlyList<Error> Errors => _errors;

    private protected Result()
    {
        IsSuccess = true;
        _errors = EmptyErrors;
        Error = null;
    }

    private protected Result(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        IsSuccess = false;
        _errors = Array.AsReadOnly([error]);
        Error = error;
    }

    private protected Result(IEnumerable<Error> errors)
    {
        var validatedErrors = ValidateAndCopyErrors(errors);

        IsSuccess = false;
        _errors = validatedErrors;
        Error = validatedErrors[0];
    }

    public static Result Success() => SuccessInstance;

    public static Result Failure(Error error) => new(error);

    public static Result Failure(IEnumerable<Error> errors) => new(errors);

    public static Result<T> Success<T>(T value) => new(value);

    public static Result<T> Failure<T>(Error error) => new(error);

    public static Result<T> Failure<T>(IEnumerable<Error> errors) => new(errors);

    private static IReadOnlyList<Error> ValidateAndCopyErrors(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var list = new List<Error>();
        foreach (var error in errors)
        {
            if (error is null)
            {
                throw new ArgumentException("Error collection cannot contain null elements.", nameof(errors));
            }

            list.Add(error);
        }

        if (list.Count == 0)
        {
            throw new ArgumentException("Error collection cannot be empty.", nameof(errors));
        }

        return Array.AsReadOnly(list.ToArray());
    }
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    internal Result(T value)
        : base()
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
    }

    internal Result(Error error)
        : base(error)
    {
        _value = default;
    }

    internal Result(IEnumerable<Error> errors)
        : base(errors)
    {
        _value = default;
    }
}
