
using Profolio.Domain.Common;
using Profolio.Domain.ValueObjects;

namespace Profolio.Domain.Entities;

public sealed class DeveloperProfile : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string Headline { get; private set; } = string.Empty;

    public string? Bio { get; private set; }

    public string? Location { get; private set; }

    public EmailAddress? PublicContactEmail { get; private set; }

    private DeveloperProfile()
    {
        // Required by EF Core for entity materialization.
    }

    public DeveloperProfile(Guid tenantId, string fullName, string headline)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

        TenantId = tenantId;
        SetBasicInformation(fullName, headline);
    }

    public void SetPublicContactEmail(EmailAddress contactEmail)
    {
        ArgumentNullException.ThrowIfNull(contactEmail);

        if (PublicContactEmail == contactEmail)
            return;

        PublicContactEmail = contactEmail;
        MarkAsUpdated();
    }

    public void ClearPublicContactEmail()
    {
        if (PublicContactEmail is null)
            return;

        PublicContactEmail = null;
        MarkAsUpdated();
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
