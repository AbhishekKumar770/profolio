
using Profolio.Domain.Common;

namespace Profolio.Domain.Entities;

public sealed class DeveloperProfile : BaseEntity
{
    public string FullName { get; private set; } = string.Empty;

    public string Headline { get; private set; } = string.Empty;

    public string? Bio { get; private set; }

    public string? Location { get; private set; }

    private DeveloperProfile()
    {
        // Required by EF Core for entity materialization.
    }

    public DeveloperProfile(string fullName, string headline)
    {
        SetBasicInformation(fullName, headline);
    }

    public void UpdateBasicInformation(
        string fullName,
        string headline,
        string? bio,
        string? location)
    {
        SetBasicInformation(fullName, headline);

        Bio = NormalizeOptionalValue(bio);
        Location = NormalizeOptionalValue(location);

        MarkAsUpdated();
    }

    private void SetBasicInformation(string fullName, string headline)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));

        if (string.IsNullOrWhiteSpace(headline))
            throw new ArgumentException("Headline is required.", nameof(headline));

        FullName = fullName.Trim();
        Headline = headline.Trim();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
