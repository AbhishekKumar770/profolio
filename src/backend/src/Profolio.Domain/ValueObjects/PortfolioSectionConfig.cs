namespace Profolio.Domain.ValueObjects;

public sealed record PortfolioSectionConfig
{
    public PortfolioSectionType SectionType { get; }

    public bool IsEnabled { get; }

    public int DisplayOrder { get; }

    public PortfolioSectionConfig(PortfolioSectionType sectionType, bool isEnabled, int displayOrder)
    {
        ArgumentNullException.ThrowIfNull(sectionType);

        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(displayOrder), displayOrder, "Display order cannot be negative.");
        }

        SectionType = sectionType;
        IsEnabled = isEnabled;
        DisplayOrder = displayOrder;
    }
}
