namespace Profolio.Domain.Entities;

using Profolio.Domain.Common;

public sealed class PublishingState : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    public bool IsPublished { get; private set; }

    public Guid? ActivePortfolioVersionId { get; private set; }

    public int? ActiveVersionNumber { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public DateTime? UnpublishedAtUtc { get; private set; }

    private PublishingState()
    {
        // Required by EF Core for entity materialization.
    }

    public PublishingState(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        TenantId = tenantId;
        IsPublished = false;
        ActivePortfolioVersionId = null;
        ActiveVersionNumber = null;
        PublishedAtUtc = null;
        UnpublishedAtUtc = null;
    }

    public void Publish(PortfolioVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);

        if (version.TenantId != TenantId)
        {
            throw new ArgumentException("Portfolio version does not belong to this tenant.", nameof(version));
        }

        if (version.Id == Guid.Empty)
        {
            throw new ArgumentException("Version ID cannot be empty.", nameof(version));
        }

        if (version.VersionNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version), version.VersionNumber, "Version number must be positive.");
        }

        if (IsPublished &&
            ActivePortfolioVersionId == version.Id &&
            ActiveVersionNumber == version.VersionNumber)
        {
            return;
        }

        IsPublished = true;
        ActivePortfolioVersionId = version.Id;
        ActiveVersionNumber = version.VersionNumber;
        PublishedAtUtc = DateTime.UtcNow;
        UnpublishedAtUtc = null;
        MarkAsUpdated();
    }

    public void Unpublish()
    {
        if (!IsPublished)
        {
            return;
        }

        IsPublished = false;
        ActivePortfolioVersionId = null;
        ActiveVersionNumber = null;
        UnpublishedAtUtc = DateTime.UtcNow;
        MarkAsUpdated();
    }
}
