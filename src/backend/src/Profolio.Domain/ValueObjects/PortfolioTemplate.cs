namespace Profolio.Domain.ValueObjects;

public sealed record PortfolioTemplate
{
    public const int MaxLength = 50;

    public static readonly PortfolioTemplate Modern = new("modern");
    public static readonly PortfolioTemplate Minimal = new("minimal");

    public string Value { get; }

    public PortfolioTemplate(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Template cannot be empty or whitespace.", nameof(value));
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Template cannot exceed {MaxLength} characters.", nameof(value));
        }

        Value = ResolveCanonicalName(trimmed);
    }

    public override string ToString() => Value;

    private static string ResolveCanonicalName(string input)
    {
        if (string.Equals(input, "modern", StringComparison.OrdinalIgnoreCase))
        {
            return "modern";
        }

        if (string.Equals(input, "minimal", StringComparison.OrdinalIgnoreCase))
        {
            return "minimal";
        }

        throw new ArgumentException($"Unsupported portfolio template '{input}'. Supported templates are 'modern' and 'minimal'.", "value");
    }
}
