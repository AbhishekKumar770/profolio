namespace Profolio.Domain.ValueObjects;

public sealed record EmailAddress
{
    private const int MaxLength = 254;

    public string Value { get; }

    public EmailAddress(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email address cannot be empty or whitespace.", nameof(value));
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Email address cannot exceed {MaxLength} characters.", nameof(value));
        }

        if (trimmed.Any(char.IsWhiteSpace))
        {
            throw new ArgumentException("Email address cannot contain whitespace.", nameof(value));
        }

        var atIndex = trimmed.IndexOf('@');
        if (atIndex <= 0 || atIndex != trimmed.LastIndexOf('@') || atIndex == trimmed.Length - 1)
        {
            throw new ArgumentException("Email address must contain exactly one '@' character separating local and domain parts.", nameof(value));
        }

        var localPart = trimmed[..atIndex];
        var rawDomainPart = trimmed[(atIndex + 1)..];

        if (string.IsNullOrEmpty(localPart) || string.IsNullOrEmpty(rawDomainPart))
        {
            throw new ArgumentException("Email address must contain non-empty local and domain parts.", nameof(value));
        }

        var domainLabels = rawDomainPart.Split('.');
        if (domainLabels.Length < 2)
        {
            throw new ArgumentException("Email domain must contain at least one dot separating labels.", nameof(value));
        }

        foreach (var label in domainLabels)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("Domain labels cannot be empty.", nameof(value));
            }

            if (label.StartsWith('-') || label.EndsWith('-'))
            {
                throw new ArgumentException("Domain labels cannot begin or end with a hyphen.", nameof(value));
            }
        }

        var normalizedDomain = rawDomainPart.ToLowerInvariant();
        Value = $"{localPart}@{normalizedDomain}";
    }

    public override string ToString() => Value;
}
