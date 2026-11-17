namespace Profolio.Domain.ValueObjects;

public sealed record PortfolioSectionType
{
    public const int MaxLength = 50;

    public static readonly PortfolioSectionType About = new("about");
    public static readonly PortfolioSectionType Skills = new("skills");
    public static readonly PortfolioSectionType Experience = new("experience");
    public static readonly PortfolioSectionType Education = new("education");
    public static readonly PortfolioSectionType Projects = new("projects");
    public static readonly PortfolioSectionType SocialLinks = new("social-links");
    public static readonly PortfolioSectionType Contact = new("contact");

    public static IReadOnlyList<PortfolioSectionType> All { get; } =
        Array.AsReadOnly(new[] { About, Skills, Experience, Education, Projects, SocialLinks, Contact });

    public string Value { get; }

    public PortfolioSectionType(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Section type cannot be empty or whitespace.", nameof(value));
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Section type cannot exceed {MaxLength} characters.", nameof(value));
        }

        Value = ResolveCanonicalName(trimmed);
    }

    public override string ToString() => Value;

    private static string ResolveCanonicalName(string input)
    {
        if (string.Equals(input, "about", StringComparison.OrdinalIgnoreCase))
        {
            return "about";
        }

        if (string.Equals(input, "skills", StringComparison.OrdinalIgnoreCase))
        {
            return "skills";
        }

        if (string.Equals(input, "experience", StringComparison.OrdinalIgnoreCase))
        {
            return "experience";
        }

        if (string.Equals(input, "education", StringComparison.OrdinalIgnoreCase))
        {
            return "education";
        }

        if (string.Equals(input, "projects", StringComparison.OrdinalIgnoreCase))
        {
            return "projects";
        }

        if (string.Equals(input, "social-links", StringComparison.OrdinalIgnoreCase))
        {
            return "social-links";
        }

        if (string.Equals(input, "contact", StringComparison.OrdinalIgnoreCase))
        {
            return "contact";
        }

        throw new ArgumentException($"Unsupported portfolio section type '{input}'.", "value");
    }
}
