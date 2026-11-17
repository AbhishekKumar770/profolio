namespace Profolio.Domain.Entities;

using Profolio.Domain.Common;
using Profolio.Domain.ValueObjects;

public sealed class PortfolioConfiguration : BaseEntity, ITenantEntity
{
    private static readonly PortfolioSectionType[] DefaultCatalog =
    [
        PortfolioSectionType.About,
        PortfolioSectionType.Skills,
        PortfolioSectionType.Experience,
        PortfolioSectionType.Education,
        PortfolioSectionType.Projects,
        PortfolioSectionType.SocialLinks,
        PortfolioSectionType.Contact
    ];

    private readonly List<PortfolioSectionConfig> _sections = [];

    public Guid TenantId { get; private set; }

    public PortfolioTemplate Template { get; private set; } = null!;

    public IReadOnlyList<PortfolioSectionConfig> Sections => _sections.AsReadOnly();

    private PortfolioConfiguration()
    {
        // Required by EF Core for entity materialization.
    }

    public PortfolioConfiguration(
        Guid tenantId,
        PortfolioTemplate? template = null,
        IEnumerable<PortfolioSectionConfig>? sections = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        var chosenTemplate = template ?? PortfolioTemplate.Modern;
        var validatedSections = ValidateAndNormalizeSections(sections, nameof(sections));

        TenantId = tenantId;
        Template = chosenTemplate;
        _sections.AddRange(validatedSections);
    }

    public void ChangeTemplate(PortfolioTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);

        if (Template == template)
        {
            return;
        }

        Template = template;
        MarkAsUpdated();
    }

    public void SetSectionVisibility(PortfolioSectionType sectionType, bool isEnabled)
    {
        ArgumentNullException.ThrowIfNull(sectionType);

        var index = _sections.FindIndex(s => s.SectionType == sectionType);
        if (index < 0)
        {
            throw new ArgumentException($"Section '{sectionType.Value}' is not part of this configuration.", nameof(sectionType));
        }

        var current = _sections[index];
        if (current.IsEnabled == isEnabled)
        {
            return;
        }

        _sections[index] = new PortfolioSectionConfig(current.SectionType, isEnabled, current.DisplayOrder);
        MarkAsUpdated();
    }

    public void ReorderSections(IEnumerable<PortfolioSectionType> orderedSectionTypes)
    {
        ArgumentNullException.ThrowIfNull(orderedSectionTypes);

        var list = new List<PortfolioSectionType>();
        var seen = new HashSet<PortfolioSectionType>();

        foreach (var item in orderedSectionTypes)
        {
            if (item is null)
            {
                throw new ArgumentException("Section type cannot be null.", nameof(orderedSectionTypes));
            }

            if (!seen.Add(item))
            {
                throw new ArgumentException($"Duplicate section '{item.Value}' in reorder list is not allowed.", nameof(orderedSectionTypes));
            }

            list.Add(item);
        }

        if (list.Count != DefaultCatalog.Length || seen.Count != DefaultCatalog.Length)
        {
            throw new ArgumentException($"Reorder list must contain exactly all {DefaultCatalog.Length} catalog sections.", nameof(orderedSectionTypes));
        }

        foreach (var catalogSection in DefaultCatalog)
        {
            if (!seen.Contains(catalogSection))
            {
                throw new ArgumentException($"Reorder list is missing section '{catalogSection.Value}'.", nameof(orderedSectionTypes));
            }
        }

        var newSections = new List<PortfolioSectionConfig>(list.Count);
        for (var i = 0; i < list.Count; i++)
        {
            var sectionType = list[i];
            var existing = _sections.First(s => s.SectionType == sectionType);
            newSections.Add(new PortfolioSectionConfig(sectionType, existing.IsEnabled, i));
        }

        if (_sections.SequenceEqual(newSections))
        {
            return;
        }

        _sections.Clear();
        _sections.AddRange(newSections);
        MarkAsUpdated();
    }

    public void UpdateSections(IEnumerable<PortfolioSectionConfig> sectionConfigs)
    {
        ArgumentNullException.ThrowIfNull(sectionConfigs);

        var validatedSections = ValidateAndNormalizeSections(sectionConfigs, nameof(sectionConfigs));

        if (_sections.SequenceEqual(validatedSections))
        {
            return;
        }

        _sections.Clear();
        _sections.AddRange(validatedSections);
        MarkAsUpdated();
    }

    private static List<PortfolioSectionConfig> ValidateAndNormalizeSections(
        IEnumerable<PortfolioSectionConfig>? sections,
        string paramName)
    {
        if (sections is null)
        {
            var defaults = new List<PortfolioSectionConfig>(DefaultCatalog.Length);
            for (var i = 0; i < DefaultCatalog.Length; i++)
            {
                defaults.Add(new PortfolioSectionConfig(DefaultCatalog[i], true, i));
            }

            return defaults;
        }

        var list = new List<PortfolioSectionConfig>();
        var seen = new HashSet<PortfolioSectionType>();

        foreach (var item in sections)
        {
            if (item is null)
            {
                throw new ArgumentException("Section configuration entry cannot be null.", paramName);
            }

            if (!seen.Add(item.SectionType))
            {
                throw new ArgumentException($"Duplicate section '{item.SectionType.Value}' is not allowed.", paramName);
            }

            list.Add(item);
        }

        if (list.Count != DefaultCatalog.Length || seen.Count != DefaultCatalog.Length)
        {
            throw new ArgumentException($"Section configurations must contain exactly all {DefaultCatalog.Length} catalog sections.", paramName);
        }

        foreach (var catalogSection in DefaultCatalog)
        {
            if (!seen.Contains(catalogSection))
            {
                throw new ArgumentException($"Missing required catalog section '{catalogSection.Value}'.", paramName);
            }
        }

        list.Sort((a, b) =>
        {
            var orderComparison = a.DisplayOrder.CompareTo(b.DisplayOrder);
            if (orderComparison != 0)
            {
                return orderComparison;
            }

            return string.Compare(a.SectionType.Value, b.SectionType.Value, StringComparison.Ordinal);
        });

        return list;
    }
}
