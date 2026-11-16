namespace Profolio.Domain.Entities;

using Profolio.Domain.Common;

public sealed class Education : BaseEntity, ITenantEntity
{
    public const int MaxInstitutionLength = 100;
    public const int MaxDegreeLength = 100;
    public const int MaxFieldOfStudyLength = 100;
    public const int MaxLocationLength = 100;
    public const int MaxDescriptionLength = 1000;

    public Guid TenantId { get; private set; }

    public string Institution { get; private set; } = string.Empty;

    public string? Degree { get; private set; }

    public string? FieldOfStudy { get; private set; }

    public string? Location { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public bool IsCurrent { get; private set; }

    public string? Description { get; private set; }

    public int DisplayOrder { get; private set; }

    private Education()
    {
        // Required by EF Core for entity materialization.
    }

    public Education(
        Guid tenantId,
        string institution,
        DateOnly startDate,
        DateOnly? endDate,
        bool isCurrent,
        string? degree = null,
        string? fieldOfStudy = null,
        string? location = null,
        string? description = null,
        int displayOrder = 0)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        TenantId = tenantId;
        Institution = ValidateAndNormalizeInstitution(institution);
        ValidateDates(startDate, endDate, isCurrent);
        StartDate = startDate;
        EndDate = endDate;
        IsCurrent = isCurrent;
        Degree = NormalizeDegree(degree);
        FieldOfStudy = NormalizeFieldOfStudy(fieldOfStudy);
        Location = NormalizeLocation(location);
        Description = NormalizeDescription(description);
        ValidateDisplayOrder(displayOrder);
        DisplayOrder = displayOrder;
    }

    public void UpdateDetails(
        string institution,
        DateOnly startDate,
        DateOnly? endDate,
        bool isCurrent,
        string? degree,
        string? fieldOfStudy,
        string? location,
        string? description,
        int displayOrder)
    {
        var normalizedInstitution = ValidateAndNormalizeInstitution(institution);
        ValidateDates(startDate, endDate, isCurrent);
        var normalizedDegree = NormalizeDegree(degree);
        var normalizedFieldOfStudy = NormalizeFieldOfStudy(fieldOfStudy);
        var normalizedLocation = NormalizeLocation(location);
        var normalizedDescription = NormalizeDescription(description);
        ValidateDisplayOrder(displayOrder);

        if (string.Equals(Institution, normalizedInstitution, StringComparison.Ordinal) &&
            StartDate == startDate &&
            EndDate == endDate &&
            IsCurrent == isCurrent &&
            string.Equals(Degree, normalizedDegree, StringComparison.Ordinal) &&
            string.Equals(FieldOfStudy, normalizedFieldOfStudy, StringComparison.Ordinal) &&
            string.Equals(Location, normalizedLocation, StringComparison.Ordinal) &&
            string.Equals(Description, normalizedDescription, StringComparison.Ordinal) &&
            DisplayOrder == displayOrder)
        {
            return;
        }

        Institution = normalizedInstitution;
        StartDate = startDate;
        EndDate = endDate;
        IsCurrent = isCurrent;
        Degree = normalizedDegree;
        FieldOfStudy = normalizedFieldOfStudy;
        Location = normalizedLocation;
        Description = normalizedDescription;
        DisplayOrder = displayOrder;
        MarkAsUpdated();
    }

    private static string ValidateAndNormalizeInstitution(string institution)
    {
        ArgumentNullException.ThrowIfNull(institution);

        var trimmed = institution.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Institution cannot be empty or whitespace.", nameof(institution));
        }

        if (trimmed.Length > MaxInstitutionLength)
        {
            throw new ArgumentException($"Institution cannot exceed {MaxInstitutionLength} characters.", nameof(institution));
        }

        return trimmed;
    }

    private static string? NormalizeDegree(string? degree)
    {
        if (string.IsNullOrWhiteSpace(degree))
        {
            return null;
        }

        var trimmed = degree.Trim();
        if (trimmed.Length > MaxDegreeLength)
        {
            throw new ArgumentException($"Degree cannot exceed {MaxDegreeLength} characters.", nameof(degree));
        }

        return trimmed;
    }

    private static string? NormalizeFieldOfStudy(string? fieldOfStudy)
    {
        if (string.IsNullOrWhiteSpace(fieldOfStudy))
        {
            return null;
        }

        var trimmed = fieldOfStudy.Trim();
        if (trimmed.Length > MaxFieldOfStudyLength)
        {
            throw new ArgumentException($"Field of study cannot exceed {MaxFieldOfStudyLength} characters.", nameof(fieldOfStudy));
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
            throw new ArgumentException("Current education cannot have an end date.", nameof(endDate));
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
