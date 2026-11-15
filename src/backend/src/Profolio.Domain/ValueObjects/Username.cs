namespace Profolio.Domain.ValueObjects;

public sealed record Username
{
    public const int MinLength = 3;
    public const int MaxLength = 30;

    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        // Core application routes
        "admin",
        "api",
        "dashboard",
        "login",
        "public",
        "register",
        "settings",
        "support",

        // Technical / SEO / Well-known paths
        "_next",
        "favicon.ico",
        "robots.txt",
        "sitemap.xml",
        "swagger"
    };

    public string Value { get; }

    public Username(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Username cannot be empty or whitespace.", nameof(value));
        }

        if (value.Any(char.IsWhiteSpace))
        {
            throw new ArgumentException("Username cannot contain whitespace.", nameof(value));
        }

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentException($"Username must be between {MinLength} and {MaxLength} characters.", nameof(value));
        }

        foreach (var c in value)
        {
            if (!IsValidCharacter(c))
            {
                throw new ArgumentException("Username can only contain ASCII letters, digits and hyphens.", nameof(value));
            }
        }

        if (value.StartsWith('-') || value.EndsWith('-'))
        {
            throw new ArgumentException("Username cannot begin or end with a hyphen.", nameof(value));
        }

        if (value.Contains("--"))
        {
            throw new ArgumentException("Username cannot contain consecutive hyphens.", nameof(value));
        }

        var normalized = value.ToLowerInvariant();

        if (ReservedNames.Contains(normalized))
        {
            throw new ArgumentException($"The username '{normalized}' is reserved and cannot be used.", nameof(value));
        }

        Value = normalized;
    }

    private static bool IsValidCharacter(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               (c >= '0' && c <= '9') ||
               c == '-';
    }

    public override string ToString() => Value;
}
