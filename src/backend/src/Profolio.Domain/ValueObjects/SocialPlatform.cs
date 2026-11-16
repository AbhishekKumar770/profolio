namespace Profolio.Domain.ValueObjects;

public sealed record SocialPlatform
{
    public const int MaxLength = 50;

    public static readonly SocialPlatform GitHub = new("GitHub");
    public static readonly SocialPlatform LinkedIn = new("LinkedIn");
    public static readonly SocialPlatform X = new("X");
    public static readonly SocialPlatform YouTube = new("YouTube");
    public static readonly SocialPlatform Instagram = new("Instagram");
    public static readonly SocialPlatform StackOverflow = new("StackOverflow");
    public static readonly SocialPlatform PersonalWebsite = new("PersonalWebsite");
    public static readonly SocialPlatform Other = new("Other");

    public string Value { get; }

    public bool RequiresLabel => string.Equals(Value, "Other", StringComparison.Ordinal);

    public SocialPlatform(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Platform cannot be empty or whitespace.", nameof(value));
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Platform cannot exceed {MaxLength} characters.", nameof(value));
        }

        Value = ResolveCanonicalName(trimmed);
    }

    public override string ToString() => Value;

    private static string ResolveCanonicalName(string input)
    {
        if (string.Equals(input, "github", StringComparison.OrdinalIgnoreCase))
            return "GitHub";

        if (string.Equals(input, "linkedin", StringComparison.OrdinalIgnoreCase))
            return "LinkedIn";

        if (string.Equals(input, "x", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(input, "twitter", StringComparison.OrdinalIgnoreCase))
            return "X";

        if (string.Equals(input, "youtube", StringComparison.OrdinalIgnoreCase))
            return "YouTube";

        if (string.Equals(input, "instagram", StringComparison.OrdinalIgnoreCase))
            return "Instagram";

        if (string.Equals(input, "stackoverflow", StringComparison.OrdinalIgnoreCase))
            return "StackOverflow";

        if (string.Equals(input, "personalwebsite", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(input, "website", StringComparison.OrdinalIgnoreCase))
            return "PersonalWebsite";

        if (string.Equals(input, "other", StringComparison.OrdinalIgnoreCase))
            return "Other";

        throw new ArgumentException($"Unsupported social platform '{input}'. Use 'Other' for custom platforms.", "value");
    }
}
