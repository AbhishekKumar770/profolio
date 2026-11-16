namespace Profolio.Domain.Entities;

using Profolio.Domain.Common;
using Profolio.Domain.ValueObjects;

public sealed class DeveloperProfile : BaseEntity, ITenantEntity
{
    public const int MaxFullNameLength = 100;
    public const int MaxHeadlineLength = 200;
    public const int MaxLocationLength = 100;

    public Guid TenantId { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string Headline { get; private set; } = string.Empty;

    public string? Bio { get; private set; }

    public string? Location { get; private set; }

    public EmailAddress? PublicContactEmail { get; private set; }

    public Guid? ProfilePhotoMediaId { get; private set; }

    private DeveloperProfile()
    {
        // Required by EF Core for entity materialization.
    }

    public DeveloperProfile(Guid tenantId, string fullName, string headline)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        var normalizedFullName = ValidateAndNormalizeFullName(fullName);
        var normalizedHeadline = ValidateAndNormalizeHeadline(headline);

        TenantId = tenantId;
        FullName = normalizedFullName;
        Headline = normalizedHeadline;
    }

    public void SetPublicContactEmail(EmailAddress contactEmail)
    {
        ArgumentNullException.ThrowIfNull(contactEmail);

        if (PublicContactEmail == contactEmail)
        {
            return;
        }

        PublicContactEmail = contactEmail;
        MarkAsUpdated();
    }

    public void ClearPublicContactEmail()
    {
        if (PublicContactEmail is null)
        {
            return;
        }

        PublicContactEmail = null;
        MarkAsUpdated();
    }

    public void SetProfilePhoto(Guid mediaId)
    {
        if (mediaId == Guid.Empty)
        {
            throw new ArgumentException("Media ID cannot be empty.", nameof(mediaId));
        }

        if (ProfilePhotoMediaId == mediaId)
        {
            return;
        }

        ProfilePhotoMediaId = mediaId;
        MarkAsUpdated();
    }

    public void RemoveProfilePhoto()
    {
        if (ProfilePhotoMediaId is null)
        {
            return;
        }

        ProfilePhotoMediaId = null;
        MarkAsUpdated();
    }

    public void UpdateBasicInformation(
        string fullName,
        string headline,
        string? bio,
        string? location)
    {
        var normalizedFullName = ValidateAndNormalizeFullName(fullName);
        var normalizedHeadline = ValidateAndNormalizeHeadline(headline);
        var normalizedBio = NormalizeBio(bio);
        var normalizedLocation = NormalizeAndValidateLocation(location);

        if (string.Equals(FullName, normalizedFullName, StringComparison.Ordinal) &&
            string.Equals(Headline, normalizedHeadline, StringComparison.Ordinal) &&
            string.Equals(Bio, normalizedBio, StringComparison.Ordinal) &&
            string.Equals(Location, normalizedLocation, StringComparison.Ordinal))
        {
            return;
        }

        FullName = normalizedFullName;
        Headline = normalizedHeadline;
        Bio = normalizedBio;
        Location = normalizedLocation;
        MarkAsUpdated();
    }

    private static string ValidateAndNormalizeFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        var trimmed = fullName.Trim();

        if (trimmed.Length > MaxFullNameLength)
        {
            throw new ArgumentException($"Full name cannot exceed {MaxFullNameLength} characters.", nameof(fullName));
        }

        return trimmed;
    }

    private static string ValidateAndNormalizeHeadline(string headline)
    {
        if (string.IsNullOrWhiteSpace(headline))
        {
            throw new ArgumentException("Headline is required.", nameof(headline));
        }

        var trimmed = headline.Trim();

        if (trimmed.Length > MaxHeadlineLength)
        {
            throw new ArgumentException($"Headline cannot exceed {MaxHeadlineLength} characters.", nameof(headline));
        }

        return trimmed;
    }

    private static string? NormalizeBio(string? bio)
    {
        if (string.IsNullOrWhiteSpace(bio))
        {
            return null;
        }

        return bio.Trim();
    }

    private static string? NormalizeAndValidateLocation(string? location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return null;
        }

        var trimmed = location.Trim();

        if (trimmed.Length > MaxLocationLength)
        {
            throw new ArgumentException($"Location cannot exceed {MaxLocationLength} characters.", nameof(location));
        }

        return trimmed;
    }
}
