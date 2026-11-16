namespace Profolio.Domain.ValueObjects;

public sealed record WebUrl
{
    public const int MaxLength = 500;

    public string Value { get; }

    public WebUrl(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Web URL cannot be empty or whitespace.", nameof(value));
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Web URL cannot exceed {MaxLength} characters.", nameof(value));
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("Web URL must be a valid absolute URL.", nameof(value));
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("Web URL must use HTTP or HTTPS scheme.", nameof(value));
        }

        if (string.IsNullOrWhiteSpace(uri.Host) || uri.HostNameType == UriHostNameType.Unknown)
        {
            throw new ArgumentException("Web URL must have a valid host.", nameof(value));
        }

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new ArgumentException("Web URL cannot contain embedded credentials.", nameof(value));
        }

        Value = uri.AbsoluteUri;
    }

    public override string ToString() => Value;
}
