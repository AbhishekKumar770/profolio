using System.Reflection;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;

namespace Profolio.UnitTests;

public class PortfolioConfigurationTests
{
    [Fact]
    public void Constructor_WithValidTenantIdAndDefaults_InitializesDefaultTemplateAndAllSevenSectionsEnabled()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var config = new PortfolioConfiguration(tenantId);

        // Assert
        Assert.NotEqual(Guid.Empty, config.Id);
        Assert.Equal(tenantId, config.TenantId);
        Assert.Equal(PortfolioTemplate.Modern, config.Template);
        Assert.True(config.CreatedAtUtc <= DateTime.UtcNow);
        Assert.Null(config.UpdatedAtUtc);

        Assert.Equal(7, config.Sections.Count);

        var expectedOrder = new[]
        {
            PortfolioSectionType.About,
            PortfolioSectionType.Skills,
            PortfolioSectionType.Experience,
            PortfolioSectionType.Education,
            PortfolioSectionType.Projects,
            PortfolioSectionType.SocialLinks,
            PortfolioSectionType.Contact
        };

        for (var i = 0; i < expectedOrder.Length; i++)
        {
            Assert.Equal(expectedOrder[i], config.Sections[i].SectionType);
            Assert.True(config.Sections[i].IsEnabled);
            Assert.Equal(i, config.Sections[i].DisplayOrder);
        }
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PortfolioConfiguration(Guid.Empty));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithExplicitTemplate_SetsChosenTemplate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var config = new PortfolioConfiguration(tenantId, PortfolioTemplate.Minimal);

        // Assert
        Assert.Equal(PortfolioTemplate.Minimal, config.Template);
    }

    [Fact]
    public void Constructor_WithExplicitCompleteSections_SetsAndSortsSectionsDeterministically()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customSections = new List<PortfolioSectionConfig>
        {
            new(PortfolioSectionType.Contact, false, 50),
            new(PortfolioSectionType.About, true, 10),
            new(PortfolioSectionType.SocialLinks, true, 40),
            new(PortfolioSectionType.Projects, false, 30),
            new(PortfolioSectionType.Education, true, 20),
            new(PortfolioSectionType.Experience, false, 15),
            new(PortfolioSectionType.Skills, true, 12)
        };

        // Act
        var config = new PortfolioConfiguration(tenantId, PortfolioTemplate.Minimal, customSections);

        // Assert
        Assert.Equal(7, config.Sections.Count);
        Assert.Equal(PortfolioSectionType.About, config.Sections[0].SectionType);
        Assert.Equal(10, config.Sections[0].DisplayOrder);
        Assert.Equal(PortfolioSectionType.Skills, config.Sections[1].SectionType);
        Assert.Equal(12, config.Sections[1].DisplayOrder);
        Assert.Equal(PortfolioSectionType.Experience, config.Sections[2].SectionType);
        Assert.Equal(15, config.Sections[2].DisplayOrder);
        Assert.Equal(PortfolioSectionType.Education, config.Sections[3].SectionType);
        Assert.Equal(20, config.Sections[3].DisplayOrder);
        Assert.Equal(PortfolioSectionType.Projects, config.Sections[4].SectionType);
        Assert.Equal(30, config.Sections[4].DisplayOrder);
        Assert.Equal(PortfolioSectionType.SocialLinks, config.Sections[5].SectionType);
        Assert.Equal(40, config.Sections[5].DisplayOrder);
        Assert.Equal(PortfolioSectionType.Contact, config.Sections[6].SectionType);
        Assert.Equal(50, config.Sections[6].DisplayOrder);
    }

    [Fact]
    public void Constructor_WithNullSectionEntry_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invalidSections = new List<PortfolioSectionConfig?>
        {
            new(PortfolioSectionType.About, true, 0),
            null,
            new(PortfolioSectionType.Experience, true, 2),
            new(PortfolioSectionType.Education, true, 3),
            new(PortfolioSectionType.Projects, true, 4),
            new(PortfolioSectionType.SocialLinks, true, 5),
            new(PortfolioSectionType.Contact, true, 6)
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new PortfolioConfiguration(tenantId, PortfolioTemplate.Modern, invalidSections!));
        Assert.Equal("sections", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithDuplicateSectionType_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var duplicateSections = new List<PortfolioSectionConfig>
        {
            new(PortfolioSectionType.About, true, 0),
            new(PortfolioSectionType.About, false, 1),
            new(PortfolioSectionType.Experience, true, 2),
            new(PortfolioSectionType.Education, true, 3),
            new(PortfolioSectionType.Projects, true, 4),
            new(PortfolioSectionType.SocialLinks, true, 5),
            new(PortfolioSectionType.Contact, true, 6)
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new PortfolioConfiguration(tenantId, PortfolioTemplate.Modern, duplicateSections));
        Assert.Equal("sections", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithMissingCatalogSection_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        // 7 items, but Skills is omitted and Contact is duplicated with different display orders
        var missingCatalog = new List<PortfolioSectionConfig>
        {
            new(PortfolioSectionType.About, true, 0),
            new(PortfolioSectionType.Contact, true, 1), // duplicate contact instead of skills
            new(PortfolioSectionType.Experience, true, 2),
            new(PortfolioSectionType.Education, true, 3),
            new(PortfolioSectionType.Projects, true, 4),
            new(PortfolioSectionType.SocialLinks, true, 5),
            new(PortfolioSectionType.Contact, true, 6)
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new PortfolioConfiguration(tenantId, PortfolioTemplate.Modern, missingCatalog));
        Assert.Equal("sections", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithFewerThanSevenSections_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var partialSections = new List<PortfolioSectionConfig>
        {
            new(PortfolioSectionType.About, true, 0),
            new(PortfolioSectionType.Skills, true, 1)
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new PortfolioConfiguration(tenantId, PortfolioTemplate.Modern, partialSections));
        Assert.Equal("sections", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithMoreThanSevenSections_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tooManySections = new List<PortfolioSectionConfig>
        {
            new(PortfolioSectionType.About, true, 0),
            new(PortfolioSectionType.Skills, true, 1),
            new(PortfolioSectionType.Experience, true, 2),
            new(PortfolioSectionType.Education, true, 3),
            new(PortfolioSectionType.Projects, true, 4),
            new(PortfolioSectionType.SocialLinks, true, 5),
            new(PortfolioSectionType.Contact, true, 6),
            new(PortfolioSectionType.About, false, 7)
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new PortfolioConfiguration(tenantId, PortfolioTemplate.Modern, tooManySections));
        Assert.Equal("sections", exception.ParamName);
    }

    // =========================================================================
    // IMMUTABILITY & ENCAPSULATION
    // =========================================================================

    [Fact]
    public void TenantId_CannotBeChangedPublicly()
    {
        // Arrange
        var property = typeof(PortfolioConfiguration).GetProperty("TenantId");

        // Assert
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }

    [Fact]
    public void Sections_ReturnsReadOnlyCollection_CannotBeMutatedExternally()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);

        // Act & Assert
        Assert.IsAssignableFrom<IReadOnlyList<PortfolioSectionConfig>>(config.Sections);
        Assert.Throws<NotSupportedException>(() =>
        {
            if (config.Sections is System.Collections.IList list)
            {
                list.Add(new PortfolioSectionConfig(PortfolioSectionType.About, false, 99));
            }
            else
            {
                throw new NotSupportedException();
            }
        });
    }

    // =========================================================================
    // TEMPLATE OPERATIONS
    // =========================================================================

    [Fact]
    public void ChangeTemplate_WithNewTemplate_UpdatesTemplateAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId, PortfolioTemplate.Modern);
        Assert.Null(config.UpdatedAtUtc);

        // Act
        config.ChangeTemplate(PortfolioTemplate.Minimal);

        // Assert
        Assert.Equal(PortfolioTemplate.Minimal, config.Template);
        Assert.NotNull(config.UpdatedAtUtc);
        Assert.True(config.UpdatedAtUtc >= config.CreatedAtUtc);
    }

    [Fact]
    public void ChangeTemplate_WithSameTemplate_DoesNotModifyEntityOrUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId, PortfolioTemplate.Modern);

        // Act
        config.ChangeTemplate(PortfolioTemplate.Modern);

        // Assert
        Assert.Equal(PortfolioTemplate.Modern, config.Template);
        Assert.Null(config.UpdatedAtUtc);
    }

    [Fact]
    public void ChangeTemplate_WhenAlreadyUpdatedWithSameTemplate_PreservesPreviousUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId, PortfolioTemplate.Modern);
        config.ChangeTemplate(PortfolioTemplate.Minimal);
        var originalUpdatedAt = config.UpdatedAtUtc;

        // Act
        config.ChangeTemplate(PortfolioTemplate.Minimal);

        // Assert
        Assert.Equal(originalUpdatedAt, config.UpdatedAtUtc);
    }

    [Fact]
    public void ChangeTemplate_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => config.ChangeTemplate(null!));
        Assert.Equal("template", exception.ParamName);
    }

    [Fact]
    public void ChangeTemplate_PreservesTenantIdAndSections()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var originalId = config.Id;
        var originalSections = config.Sections.ToList();

        // Act
        config.ChangeTemplate(PortfolioTemplate.Minimal);

        // Assert
        Assert.Equal(originalId, config.Id);
        Assert.Equal(tenantId, config.TenantId);
        Assert.Equal(originalSections, config.Sections);
    }

    // =========================================================================
    // VISIBILITY OPERATIONS
    // =========================================================================

    [Fact]
    public void SetSectionVisibility_DisablingSection_UpdatesIsEnabledAndPreservesDisplayOrder()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        Assert.True(config.Sections[1].IsEnabled);
        Assert.Equal(1, config.Sections[1].DisplayOrder);

        // Act
        config.SetSectionVisibility(PortfolioSectionType.Skills, false);

        // Assert
        var skills = config.Sections.First(s => s.SectionType == PortfolioSectionType.Skills);
        Assert.False(skills.IsEnabled);
        Assert.Equal(1, skills.DisplayOrder);
        Assert.NotNull(config.UpdatedAtUtc);
    }

    [Fact]
    public void SetSectionVisibility_EnablingDisabledSection_UpdatesIsEnabledAndPreservesDisplayOrder()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var initialSections = new List<PortfolioSectionConfig>
        {
            new(PortfolioSectionType.About, true, 0),
            new(PortfolioSectionType.Skills, true, 1),
            new(PortfolioSectionType.Experience, true, 2),
            new(PortfolioSectionType.Education, true, 3),
            new(PortfolioSectionType.Projects, false, 4), // disabled at construction
            new(PortfolioSectionType.SocialLinks, true, 5),
            new(PortfolioSectionType.Contact, true, 6)
        };

        var config = new PortfolioConfiguration(tenantId, PortfolioTemplate.Modern, initialSections);
        Assert.Null(config.UpdatedAtUtc);
        Assert.False(config.Sections.First(s => s.SectionType == PortfolioSectionType.Projects).IsEnabled);

        // Act
        config.SetSectionVisibility(PortfolioSectionType.Projects, true);

        // Assert
        var projects = config.Sections.First(s => s.SectionType == PortfolioSectionType.Projects);
        Assert.True(projects.IsEnabled);
        Assert.Equal(4, projects.DisplayOrder);
        Assert.NotNull(config.UpdatedAtUtc);
        Assert.True(config.UpdatedAtUtc >= config.CreatedAtUtc);
    }

    [Fact]
    public void SetSectionVisibility_WithSameVisibilityState_DoesNotModifyEntityOrUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);

        // Act
        config.SetSectionVisibility(PortfolioSectionType.About, true);

        // Assert
        Assert.Null(config.UpdatedAtUtc);
        Assert.True(config.Sections.First(s => s.SectionType == PortfolioSectionType.About).IsEnabled);
    }

    [Fact]
    public void SetSectionVisibility_WhenAlreadyUpdatedWithSameVisibility_PreservesPreviousUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        config.SetSectionVisibility(PortfolioSectionType.Skills, false);
        var originalUpdatedAt = config.UpdatedAtUtc;
        Assert.NotNull(originalUpdatedAt);

        // Act - call again with same visibility (false)
        config.SetSectionVisibility(PortfolioSectionType.Skills, false);

        // Assert
        Assert.Equal(originalUpdatedAt, config.UpdatedAtUtc);
        var skills = config.Sections.First(s => s.SectionType == PortfolioSectionType.Skills);
        Assert.False(skills.IsEnabled);
    }

    [Fact]
    public void SetSectionVisibility_WithNullSectionType_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => config.SetSectionVisibility(null!, false));
        Assert.Equal("sectionType", exception.ParamName);
    }

    [Fact]
    public void SetSectionVisibility_PreservesOtherSections()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var otherSectionsBefore = config.Sections.Where(s => s.SectionType != PortfolioSectionType.Experience).ToList();

        // Act
        config.SetSectionVisibility(PortfolioSectionType.Experience, false);

        // Assert
        var otherSectionsAfter = config.Sections.Where(s => s.SectionType != PortfolioSectionType.Experience).ToList();
        Assert.Equal(otherSectionsBefore, otherSectionsAfter);
    }

    // =========================================================================
    // REORDERING OPERATIONS
    // =========================================================================

    [Fact]
    public void ReorderSections_WithValidNewOrder_AssignsConsecutiveDisplayOrdersStartingAtZeroAndPreservesVisibility()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var initialSections = new List<PortfolioSectionConfig>
        {
            new(PortfolioSectionType.About, true, 0),
            new(PortfolioSectionType.Skills, true, 1),
            new(PortfolioSectionType.Experience, true, 2),
            new(PortfolioSectionType.Education, false, 3), // disabled at construction
            new(PortfolioSectionType.Projects, true, 4),
            new(PortfolioSectionType.SocialLinks, true, 5),
            new(PortfolioSectionType.Contact, true, 6)
        };

        var config = new PortfolioConfiguration(tenantId, PortfolioTemplate.Modern, initialSections);
        Assert.Null(config.UpdatedAtUtc);

        var newOrder = new[]
        {
            PortfolioSectionType.Projects,
            PortfolioSectionType.Contact,
            PortfolioSectionType.Education,
            PortfolioSectionType.About,
            PortfolioSectionType.Skills,
            PortfolioSectionType.Experience,
            PortfolioSectionType.SocialLinks
        };

        // Act
        config.ReorderSections(newOrder);

        // Assert
        Assert.NotNull(config.UpdatedAtUtc);
        Assert.True(config.UpdatedAtUtc >= config.CreatedAtUtc);
        Assert.Equal(7, config.Sections.Count);
        for (var i = 0; i < newOrder.Length; i++)
        {
            Assert.Equal(newOrder[i], config.Sections[i].SectionType);
            Assert.Equal(i, config.Sections[i].DisplayOrder);
        }

        // Verify Education preserved its disabled state
        var education = config.Sections.First(s => s.SectionType == PortfolioSectionType.Education);
        Assert.False(education.IsEnabled);
        Assert.Equal(2, education.DisplayOrder);

        // Verify Projects preserved its enabled state
        var projects = config.Sections.First(s => s.SectionType == PortfolioSectionType.Projects);
        Assert.True(projects.IsEnabled);
        Assert.Equal(0, projects.DisplayOrder);
    }

    [Fact]
    public void ReorderSections_WithSameOrder_DoesNotModifyEntityOrUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var currentOrder = config.Sections.Select(s => s.SectionType).ToList();

        // Act
        config.ReorderSections(currentOrder);

        // Assert
        Assert.Null(config.UpdatedAtUtc);
    }

    [Fact]
    public void ReorderSections_WhenAlreadyUpdatedWithSameOrder_PreservesPreviousUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var newOrder = new[]
        {
            PortfolioSectionType.Projects,
            PortfolioSectionType.Contact,
            PortfolioSectionType.Education,
            PortfolioSectionType.About,
            PortfolioSectionType.Skills,
            PortfolioSectionType.Experience,
            PortfolioSectionType.SocialLinks
        };
        config.ReorderSections(newOrder);
        var originalUpdatedAt = config.UpdatedAtUtc;
        Assert.NotNull(originalUpdatedAt);

        // Act - call again with the exact same order
        config.ReorderSections(newOrder);

        // Assert
        Assert.Equal(originalUpdatedAt, config.UpdatedAtUtc);
    }

    [Fact]
    public void ReorderSections_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => config.ReorderSections(null!));
        Assert.Equal("orderedSectionTypes", exception.ParamName);
    }

    [Fact]
    public void ReorderSections_WithNullEntryInList_ThrowsArgumentExceptionAndPreservesState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var originalSections = config.Sections.ToList();

        var invalidOrder = new PortfolioSectionType?[]
        {
            PortfolioSectionType.Projects,
            null,
            PortfolioSectionType.About,
            PortfolioSectionType.Skills,
            PortfolioSectionType.Experience,
            PortfolioSectionType.Education,
            PortfolioSectionType.SocialLinks
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => config.ReorderSections(invalidOrder!));
        Assert.Equal("orderedSectionTypes", exception.ParamName);
        Assert.Equal(originalSections, config.Sections);
        Assert.Null(config.UpdatedAtUtc);
    }

    [Fact]
    public void ReorderSections_WithDuplicateSectionInList_ThrowsArgumentExceptionAndPreservesState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var originalSections = config.Sections.ToList();

        var duplicateOrder = new[]
        {
            PortfolioSectionType.Projects,
            PortfolioSectionType.Projects,
            PortfolioSectionType.About,
            PortfolioSectionType.Skills,
            PortfolioSectionType.Experience,
            PortfolioSectionType.Education,
            PortfolioSectionType.SocialLinks
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => config.ReorderSections(duplicateOrder));
        Assert.Equal("orderedSectionTypes", exception.ParamName);
        Assert.Equal(originalSections, config.Sections);
        Assert.Null(config.UpdatedAtUtc);
    }

    [Fact]
    public void ReorderSections_WithMissingSectionInList_ThrowsArgumentExceptionAndPreservesState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var originalSections = config.Sections.ToList();

        var incompleteOrder = new[]
        {
            PortfolioSectionType.Projects,
            PortfolioSectionType.About,
            PortfolioSectionType.Skills
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => config.ReorderSections(incompleteOrder));
        Assert.Equal("orderedSectionTypes", exception.ParamName);
        Assert.Equal(originalSections, config.Sections);
        Assert.Null(config.UpdatedAtUtc);
    }

    [Fact]
    public void ReorderSections_WhenDeferredEnumeratorThrowsMidway_PreservesAllEntityState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var originalSections = config.Sections.ToList();

        static IEnumerable<PortfolioSectionType> ThrowingSequence()
        {
            yield return PortfolioSectionType.Projects;
            yield return PortfolioSectionType.About;
            throw new InvalidOperationException("Deferred iteration failure");
        }

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => config.ReorderSections(ThrowingSequence()));
        Assert.Equal(originalSections, config.Sections);
        Assert.Null(config.UpdatedAtUtc);
    }

    // =========================================================================
    // BULK UPDATE SECTIONS OPERATIONS
    // =========================================================================

    [Fact]
    public void UpdateSections_WithValidNewConfigurations_UpdatesAllSectionsAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);

        var newConfigs = new List<PortfolioSectionConfig>
        {
            new(PortfolioSectionType.Contact, false, 100),
            new(PortfolioSectionType.Projects, true, 10),
            new(PortfolioSectionType.About, true, 20),
            new(PortfolioSectionType.Skills, false, 30),
            new(PortfolioSectionType.Experience, true, 40),
            new(PortfolioSectionType.Education, false, 50),
            new(PortfolioSectionType.SocialLinks, true, 60)
        };

        // Act
        config.UpdateSections(newConfigs);

        // Assert
        Assert.Equal(7, config.Sections.Count);
        Assert.Equal(PortfolioSectionType.Projects, config.Sections[0].SectionType);
        Assert.True(config.Sections[0].IsEnabled);
        Assert.Equal(10, config.Sections[0].DisplayOrder);

        Assert.Equal(PortfolioSectionType.Contact, config.Sections[6].SectionType);
        Assert.False(config.Sections[6].IsEnabled);
        Assert.Equal(100, config.Sections[6].DisplayOrder);

        Assert.NotNull(config.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateSections_SortsSectionsDeterministicallyByDisplayOrderAndSectionType()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);

        // Two items share DisplayOrder 10: "about" vs "skills" -> "about" comes first ordinally
        var newConfigs = new List<PortfolioSectionConfig>
        {
            new(PortfolioSectionType.Skills, true, 10),
            new(PortfolioSectionType.About, true, 10),
            new(PortfolioSectionType.Experience, true, 20),
            new(PortfolioSectionType.Education, true, 30),
            new(PortfolioSectionType.Projects, true, 40),
            new(PortfolioSectionType.SocialLinks, true, 50),
            new(PortfolioSectionType.Contact, true, 60)
        };

        // Act
        config.UpdateSections(newConfigs);

        // Assert
        Assert.Equal(PortfolioSectionType.About, config.Sections[0].SectionType);
        Assert.Equal(10, config.Sections[0].DisplayOrder);
        Assert.Equal(PortfolioSectionType.Skills, config.Sections[1].SectionType);
        Assert.Equal(10, config.Sections[1].DisplayOrder);
    }

    [Fact]
    public void UpdateSections_WithIdenticalConfigurations_DoesNotModifyEntityOrUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var currentConfigs = config.Sections.ToList();

        // Act
        config.UpdateSections(currentConfigs);

        // Assert
        Assert.Null(config.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateSections_WhenAlreadyUpdatedWithSameConfigurations_PreservesPreviousUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var newConfigs = new List<PortfolioSectionConfig>
        {
            new(PortfolioSectionType.Contact, false, 100),
            new(PortfolioSectionType.Projects, true, 10),
            new(PortfolioSectionType.About, true, 20),
            new(PortfolioSectionType.Skills, false, 30),
            new(PortfolioSectionType.Experience, true, 40),
            new(PortfolioSectionType.Education, false, 50),
            new(PortfolioSectionType.SocialLinks, true, 60)
        };
        config.UpdateSections(newConfigs);
        var originalUpdatedAt = config.UpdatedAtUtc;
        Assert.NotNull(originalUpdatedAt);

        // Act - call again with the exact same configurations
        config.UpdateSections(newConfigs);

        // Assert
        Assert.Equal(originalUpdatedAt, config.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateSections_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => config.UpdateSections(null!));
        Assert.Equal("sectionConfigs", exception.ParamName);
    }

    [Fact]
    public void UpdateSections_WithNullItemInList_ThrowsArgumentExceptionAndPreservesState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var originalSections = config.Sections.ToList();

        var invalidList = new List<PortfolioSectionConfig?>
        {
            new(PortfolioSectionType.About, true, 0),
            null,
            new(PortfolioSectionType.Experience, true, 2),
            new(PortfolioSectionType.Education, true, 3),
            new(PortfolioSectionType.Projects, true, 4),
            new(PortfolioSectionType.SocialLinks, true, 5),
            new(PortfolioSectionType.Contact, true, 6)
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => config.UpdateSections(invalidList!));
        Assert.Equal("sectionConfigs", exception.ParamName);
        Assert.Equal(originalSections, config.Sections);
        Assert.Null(config.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateSections_WithDuplicateSectionType_ThrowsArgumentExceptionAndPreservesState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var originalSections = config.Sections.ToList();

        var duplicateList = new List<PortfolioSectionConfig>
        {
            new(PortfolioSectionType.About, true, 0),
            new(PortfolioSectionType.About, false, 1),
            new(PortfolioSectionType.Experience, true, 2),
            new(PortfolioSectionType.Education, true, 3),
            new(PortfolioSectionType.Projects, true, 4),
            new(PortfolioSectionType.SocialLinks, true, 5),
            new(PortfolioSectionType.Contact, true, 6)
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => config.UpdateSections(duplicateList));
        Assert.Equal("sectionConfigs", exception.ParamName);
        Assert.Equal(originalSections, config.Sections);
        Assert.Null(config.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateSections_WithMissingCatalogSection_ThrowsArgumentExceptionAndPreservesState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var originalSections = config.Sections.ToList();

        var incompleteList = new List<PortfolioSectionConfig>
        {
            new(PortfolioSectionType.About, true, 0),
            new(PortfolioSectionType.Skills, true, 1)
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => config.UpdateSections(incompleteList));
        Assert.Equal("sectionConfigs", exception.ParamName);
        Assert.Equal(originalSections, config.Sections);
        Assert.Null(config.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateSections_WhenDeferredEnumeratorThrowsMidway_PreservesAllEntityState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new PortfolioConfiguration(tenantId);
        var originalSections = config.Sections.ToList();

        static IEnumerable<PortfolioSectionConfig> ThrowingConfigs()
        {
            yield return new PortfolioSectionConfig(PortfolioSectionType.About, true, 0);
            yield return new PortfolioSectionConfig(PortfolioSectionType.Skills, true, 1);
            throw new InvalidOperationException("Deferred config iteration failure");
        }

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => config.UpdateSections(ThrowingConfigs()));
        Assert.Equal(originalSections, config.Sections);
        Assert.Null(config.UpdatedAtUtc);
    }
}
