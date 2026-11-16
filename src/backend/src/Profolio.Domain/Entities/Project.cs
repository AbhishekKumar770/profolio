namespace Profolio.Domain.Entities;

using Profolio.Domain.Common;
using Profolio.Domain.ValueObjects;

public sealed class Project : BaseEntity, ITenantEntity
{
    public const int MaxTitleLength = 150;
    public const int MaxShortDescriptionLength = 300;
    public const int MaxDescriptionLength = 5000;
    public const int MaxTechnologiesCount = 20;
    public const int MaxTechnologyLength = 50;

    private readonly List<string> _technologies = [];

    public Guid TenantId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? ShortDescription { get; private set; }

    public string? Description { get; private set; }

    public WebUrl? GitHubUrl { get; private set; }

    public WebUrl? LiveUrl { get; private set; }

    public bool IsFeatured { get; private set; }

    public int DisplayOrder { get; private set; }

    public IReadOnlyList<string> Technologies => _technologies.AsReadOnly();

    private Project()
    {
        // Required by EF Core for entity materialization.
    }

    public Project(
        Guid tenantId,
        string title,
        string? shortDescription = null,
        string? description = null,
        WebUrl? gitHubUrl = null,
        WebUrl? liveUrl = null,
        bool isFeatured = false,
        int displayOrder = 0,
        IEnumerable<string>? technologies = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        var normalizedTitle = ValidateAndNormalizeTitle(title);
        var normalizedShortDescription = NormalizeShortDescription(shortDescription);
        var normalizedDescription = NormalizeDescription(description);
        ValidateDisplayOrder(displayOrder);
        var validatedTechnologies = ValidateAndNormalizeTechnologies(technologies);

        TenantId = tenantId;
        Title = normalizedTitle;
        ShortDescription = normalizedShortDescription;
        Description = normalizedDescription;
        GitHubUrl = gitHubUrl;
        LiveUrl = liveUrl;
        IsFeatured = isFeatured;
        DisplayOrder = displayOrder;
        _technologies.AddRange(validatedTechnologies);
    }

    public void UpdateDetails(
        string title,
        string? shortDescription,
        string? description,
        WebUrl? gitHubUrl,
        WebUrl? liveUrl,
        bool isFeatured,
        int displayOrder,
        IEnumerable<string>? technologies)
    {
        var normalizedTitle = ValidateAndNormalizeTitle(title);
        var normalizedShortDescription = NormalizeShortDescription(shortDescription);
        var normalizedDescription = NormalizeDescription(description);
        ValidateDisplayOrder(displayOrder);
        var validatedTechnologies = ValidateAndNormalizeTechnologies(technologies);

        if (string.Equals(Title, normalizedTitle, StringComparison.Ordinal) &&
            string.Equals(ShortDescription, normalizedShortDescription, StringComparison.Ordinal) &&
            string.Equals(Description, normalizedDescription, StringComparison.Ordinal) &&
            GitHubUrl == gitHubUrl &&
            LiveUrl == liveUrl &&
            IsFeatured == isFeatured &&
            DisplayOrder == displayOrder &&
            _technologies.SequenceEqual(validatedTechnologies, StringComparer.Ordinal))
        {
            return;
        }

        Title = normalizedTitle;
        ShortDescription = normalizedShortDescription;
        Description = normalizedDescription;
        GitHubUrl = gitHubUrl;
        LiveUrl = liveUrl;
        IsFeatured = isFeatured;
        DisplayOrder = displayOrder;
        _technologies.Clear();
        _technologies.AddRange(validatedTechnologies);
        MarkAsUpdated();
    }

    private static string ValidateAndNormalizeTitle(string title)
    {
        ArgumentNullException.ThrowIfNull(title);

        var trimmed = title.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Title cannot be empty or whitespace.", nameof(title));
        }

        if (trimmed.Length > MaxTitleLength)
        {
            throw new ArgumentException($"Title cannot exceed {MaxTitleLength} characters.", nameof(title));
        }

        return trimmed;
    }

    private static string? NormalizeShortDescription(string? shortDescription)
    {
        if (string.IsNullOrWhiteSpace(shortDescription))
        {
            return null;
        }

        var trimmed = shortDescription.Trim();
        if (trimmed.Length > MaxShortDescriptionLength)
        {
            throw new ArgumentException($"Short description cannot exceed {MaxShortDescriptionLength} characters.", nameof(shortDescription));
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

    private static void ValidateDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(displayOrder), displayOrder, "Display order cannot be negative.");
        }
    }

    private static List<string> ValidateAndNormalizeTechnologies(IEnumerable<string>? technologies)
    {
        if (technologies is null)
        {
            return [];
        }

        var list = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in technologies)
        {
            if (list.Count >= MaxTechnologiesCount)
            {
                throw new ArgumentException($"A project cannot have more than {MaxTechnologiesCount} technologies.", nameof(technologies));
            }

            if (item is null)
            {
                throw new ArgumentException("Technology name cannot be null.", nameof(technologies));
            }

            var trimmed = item.Trim();
            if (trimmed.Length == 0)
            {
                throw new ArgumentException("Technology name cannot be empty or whitespace.", nameof(technologies));
            }

            if (trimmed.Length > MaxTechnologyLength)
            {
                throw new ArgumentException($"Technology name cannot exceed {MaxTechnologyLength} characters.", nameof(technologies));
            }

            if (!seen.Add(trimmed))
            {
                throw new ArgumentException($"Duplicate technology '{trimmed}' is not allowed.", nameof(technologies));
            }

            list.Add(trimmed);
        }

        return list;
    }
}
