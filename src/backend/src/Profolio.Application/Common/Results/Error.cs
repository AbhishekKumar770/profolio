namespace Profolio.Application.Common.Results;

public sealed record Error
{
    public string Code { get; }

    public string Description { get; }

    public ErrorType Type { get; }

    public string? Field { get; }

    public Error(string code, string description, ErrorType type, string? field = null)
    {
        ArgumentNullException.ThrowIfNull(code);

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Error code cannot be empty or whitespace.", nameof(code));
        }

        ArgumentNullException.ThrowIfNull(description);

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Error description cannot be empty or whitespace.", nameof(description));
        }

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Error type must be a defined ErrorType value.");
        }

        Code = code.Trim();
        Description = description.Trim();
        Type = type;
        Field = string.IsNullOrWhiteSpace(field) ? null : field.Trim();
    }

    public static Error Validation(string code, string description, string? field = null) =>
        new(code, description, ErrorType.Validation, field);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Unauthorized(string code, string description) =>
        new(code, description, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);
}
