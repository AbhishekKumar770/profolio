namespace Profolio.Domain.Entities;

using Profolio.Domain.Common;

public sealed class WorkExperience : BaseEntity, ITenantEntity
{
    public const int MaxCompanyNameLength = 100;
    public const int MaxRoleLength = 100;
    public const int MaxLocationLength = 100;
    public const int MaxDescriptionLength = 2000;

    public Guid TenantId { get; private set; }

    public string CompanyName { get; private set; } = string.Empty;

    public string Role { get; private set; } = string.Empty;

    public string? Location { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public bool IsCurrent { get; private set; }

    public string? Description { get; private set; }

    public int DisplayOrder { get; private set; }

    private WorkExperience()
    {
        // Required by EF Core for entity materialization.
    }

    public WorkExperience(
        Guid tenantId,
        string companyName,
        string role,
        DateOnly startDate,
        DateOnly? endDate,
        bool isCurrent,
        string? location = null,
        string? description = null,
        int displayOrder = 0)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        TenantId = tenantId;
        CompanyName = ValidateAndNormalizeCompanyName(companyName);
        Role = ValidateAndNormalizeRole(role);
        Location = NormalizeLocation(location);
        ValidateDates(startDate, endDate, isCurrent);
        StartDate = startDate;
        EndDate = endDate;
        IsCurrent = isCurrent;
        Description = NormalizeDescription(description);
        ValidateDisplayOrder(displayOrder);
        DisplayOrder = displayOrder;
    }

    public void UpdateDetails(
        string companyName,
        string role,
        DateOnly startDate,
        DateOnly? endDate,
        bool isCurrent,
        string? location,
        string? description,
        int displayOrder)
    {
        var normalizedCompanyName = ValidateAndNormalizeCompanyName(companyName);
        var normalizedRole = ValidateAndNormalizeRole(role);
        var normalizedLocation = NormalizeLocation(location);
        ValidateDates(startDate, endDate, isCurrent);
        var normalizedDescription = NormalizeDescription(description);
        ValidateDisplayOrder(displayOrder);

        if (string.Equals(CompanyName, normalizedCompanyName, StringComparison.Ordinal) &&
            string.Equals(Role, normalizedRole, StringComparison.Ordinal) &&
            string.Equals(Location, normalizedLocation, StringComparison.Ordinal) &&
            StartDate == startDate &&
            EndDate == endDate &&
            IsCurrent == isCurrent &&
            string.Equals(Description, normalizedDescription, StringComparison.Ordinal) &&
            DisplayOrder == displayOrder)
        {
            return;
        }

        CompanyName = normalizedCompanyName;
        Role = normalizedRole;
        Location = normalizedLocation;
        StartDate = startDate;
        EndDate = endDate;
        IsCurrent = isCurrent;
        Description = normalizedDescription;
        DisplayOrder = displayOrder;
        MarkAsUpdated();
    }

    private static string ValidateAndNormalizeCompanyName(string companyName)
    {
        ArgumentNullException.ThrowIfNull(companyName);

        var trimmed = companyName.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Company name cannot be empty or whitespace.", nameof(companyName));
        }

        if (trimmed.Length > MaxCompanyNameLength)
        {
            throw new ArgumentException($"Company name cannot exceed {MaxCompanyNameLength} characters.", nameof(companyName));
        }

        return trimmed;
    }

    private static string ValidateAndNormalizeRole(string role)
    {
        ArgumentNullException.ThrowIfNull(role);

        var trimmed = role.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Role cannot be empty or whitespace.", nameof(role));
        }

        if (trimmed.Length > MaxRoleLength)
        {
            throw new ArgumentException($"Role cannot exceed {MaxRoleLength} characters.", nameof(role));
        }

        return trimmed;
    }

    private static string? NormalizeLocation(string? location)
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

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var trimmed = description.Trim();
        if (trimmed.Length > MaxDescriptionLength)
        {
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters.", nameof(description));
        }

        return trimmed;
    }

    private static void ValidateDates(DateOnly startDate, DateOnly? endDate, bool isCurrent)
    {
        if (startDate == DateOnly.MinValue)
        {
            throw new ArgumentException("Start date is required.", nameof(startDate));
        }

        if (isCurrent && endDate.HasValue)
        {
            throw new ArgumentException("Current employment cannot have an end date.", nameof(endDate));
        }

        if (endDate.HasValue && endDate.Value < startDate)
        {
            throw new ArgumentException("End date cannot be earlier than start date.", nameof(endDate));
        }
    }

    private static void ValidateDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(displayOrder), displayOrder, "Display order cannot be negative.");
        }
    }
}
