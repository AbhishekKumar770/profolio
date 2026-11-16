namespace Profolio.Domain.Entities;

using Profolio.Domain.Common;
using Profolio.Domain.ValueObjects;

public sealed class SocialLink : BaseEntity, ITenantEntity
{
    public const int MaxLabelLength = 100;

    public Guid TenantId { get; private set; }

    public SocialPlatform Platform { get; private set; } = null!;

    public WebUrl Url { get; private set; } = null!;

    public string? Label { get; private set; }

    public int DisplayOrder { get; private set; }

    private SocialLink()
    {
        // Required by EF Core for entity materialization.
    }

    public SocialLink(
        Guid tenantId,
        SocialPlatform platform,
        WebUrl url,
        string? label = null,
        int displayOrder = 0)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        ArgumentNullException.ThrowIfNull(platform);
        ArgumentNullException.ThrowIfNull(url);

        var normalizedLabel = NormalizeAndValidateLabel(label, platform);
        ValidateDisplayOrder(displayOrder);

        TenantId = tenantId;
        Platform = platform;
        Url = url;
        Label = normalizedLabel;
        DisplayOrder = displayOrder;
    }

    public void UpdateDetails(
        SocialPlatform platform,
        WebUrl url,
        string? label,
        int displayOrder)
    {
        ArgumentNullException.ThrowIfNull(platform);
        ArgumentNullException.ThrowIfNull(url);

        var normalizedLabel = NormalizeAndValidateLabel(label, platform);
        ValidateDisplayOrder(displayOrder);

        if (Platform == platform &&
            Url == url &&
            string.Equals(Label, normalizedLabel, StringComparison.Ordinal) &&
            DisplayOrder == displayOrder)
        {
            return;
        }

        Platform = platform;
        Url = url;
        Label = normalizedLabel;
        DisplayOrder = displayOrder;
        MarkAsUpdated();
    }

    private static string? NormalizeAndValidateLabel(string? label, SocialPlatform platform)
    {
        var trimmed = label?.Trim();
        var isBlank = string.IsNullOrWhiteSpace(trimmed);

        if (platform.RequiresLabel && isBlank)
        {
            throw new ArgumentException("Label is required when platform is Other.", nameof(label));
        }

        if (isBlank)
        {
            return null;
        }

        if (trimmed!.Length > MaxLabelLength)
        {
            throw new ArgumentException($"Label cannot exceed {MaxLabelLength} characters.", nameof(label));
        }

        return trimmed;
    }

    private static void ValidateDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(displayOrder), displayOrder, "Display order cannot be negative.");
        }
    }
}
