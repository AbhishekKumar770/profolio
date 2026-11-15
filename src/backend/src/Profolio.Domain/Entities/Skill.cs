namespace Profolio.Domain.Entities;

using Profolio.Domain.Common;

public sealed class Skill : BaseEntity, ITenantEntity
{
    public const int MaxNameLength = 50;
    public const int MaxCategoryLength = 50;

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Category { get; private set; }

    public int DisplayOrder { get; private set; }

    private Skill()
    {
        // Required by EF Core for entity materialization.
    }

    public Skill(Guid tenantId, string name, string? category = null, int displayOrder = 0)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        TenantId = tenantId;
        Name = ValidateAndNormalizeName(name);
        Category = NormalizeCategory(category);
        ValidateDisplayOrder(displayOrder);
        DisplayOrder = displayOrder;
    }

    public void UpdateDetails(string name, string? category, int displayOrder)
    {
        var normalizedName = ValidateAndNormalizeName(name);
        var normalizedCategory = NormalizeCategory(category);
        ValidateDisplayOrder(displayOrder);

        if (string.Equals(Name, normalizedName, StringComparison.Ordinal) &&
            string.Equals(Category, normalizedCategory, StringComparison.Ordinal) &&
            DisplayOrder == displayOrder)
        {
            return;
        }

        Name = normalizedName;
        Category = normalizedCategory;
        DisplayOrder = displayOrder;
        MarkAsUpdated();
    }

    private static string ValidateAndNormalizeName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var trimmed = name.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Skill name cannot be empty or whitespace.", nameof(name));
        }

        if (trimmed.Length > MaxNameLength)
        {
            throw new ArgumentException($"Skill name cannot exceed {MaxNameLength} characters.", nameof(name));
        }

        return trimmed;
    }

    private static string? NormalizeCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return null;
        }

        var trimmed = category.Trim();
        if (trimmed.Length > MaxCategoryLength)
        {
            throw new ArgumentException($"Skill category cannot exceed {MaxCategoryLength} characters.", nameof(category));
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
