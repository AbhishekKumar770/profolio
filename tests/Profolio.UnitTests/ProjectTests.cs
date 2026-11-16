using Profolio.Domain.Common;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;

namespace Profolio.UnitTests;

public class ProjectTests
{
    [Fact]
    public void Constructor_WithAllValidArguments_CreatesProject()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var title = "Profolio Portfolio Platform";
        var shortDescription = "A clean developer portfolio engine.";
        var description = "Full architectural implementation of Profolio using Clean Architecture.";
        var gitHubUrl = new WebUrl("https://github.com/developer/profolio");
        var liveUrl = new WebUrl("https://profolio.dev/showcase");
        var isFeatured = true;
        var displayOrder = 2;
        var technologies = new[] { "C#", ".NET 10", "PostgreSQL" };

        // Act
        var project = new Project(
            tenantId,
            title,
            shortDescription,
            description,
            gitHubUrl,
            liveUrl,
            isFeatured,
            displayOrder,
            technologies);

        // Assert
        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.Equal(tenantId, project.TenantId);
        Assert.Equal(title, project.Title);
        Assert.Equal(shortDescription, project.ShortDescription);
        Assert.Equal(description, project.Description);
        Assert.Equal(gitHubUrl, project.GitHubUrl);
        Assert.Equal(liveUrl, project.LiveUrl);
        Assert.True(project.IsFeatured);
        Assert.Equal(2, project.DisplayOrder);
        Assert.Equal(3, project.Technologies.Count);
        Assert.Equal(new[] { "C#", ".NET 10", "PostgreSQL" }, project.Technologies);
        Assert.True(project.CreatedAtUtc <= DateTime.UtcNow);
        Assert.Null(project.UpdatedAtUtc);
        Assert.IsAssignableFrom<ITenantEntity>(project);
        Assert.IsAssignableFrom<BaseEntity>(project);
    }

    [Fact]
    public void Constructor_WithRequiredArgumentsOnly_AppliesDefaultValues()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var title = "Minimal Project";

        // Act
        var project = new Project(tenantId, title);

        // Assert
        Assert.Equal(tenantId, project.TenantId);
        Assert.Equal("Minimal Project", project.Title);
        Assert.Null(project.ShortDescription);
        Assert.Null(project.Description);
        Assert.Null(project.GitHubUrl);
        Assert.Null(project.LiveUrl);
        Assert.False(project.IsFeatured);
        Assert.Equal(0, project.DisplayOrder);
        Assert.Empty(project.Technologies);
        Assert.Null(project.UpdatedAtUtc);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Project(emptyTenantId, "Title"));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullTitle_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Project(tenantId, null!));
        Assert.Equal("title", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithEmptyOrWhitespaceTitle_ThrowsArgumentException(string invalidTitle)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Project(tenantId, invalidTitle));
        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithUntrimmedTitle_TrimsWhitespace()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var rawTitle = "   Trimmed Project Title   ";

        // Act
        var project = new Project(tenantId, rawTitle);

        // Assert
        Assert.Equal("Trimmed Project Title", project.Title);
    }

    [Fact]
    public void Constructor_WithTitleAtMaximumLength_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var maxTitle = new string('T', Project.MaxTitleLength);

        // Act
        var project = new Project(tenantId, maxTitle);

        // Assert
        Assert.Equal(maxTitle, project.Title);
        Assert.Equal(Project.MaxTitleLength, project.Title.Length);
    }

    [Fact]
    public void Constructor_WithTitleAtMaximumLengthSurroundedByWhitespace_TrimsAndSucceeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var maxTitle = new string('T', Project.MaxTitleLength);
        var paddedTitle = $"   {maxTitle}   ";

        // Act
        var project = new Project(tenantId, paddedTitle);

        // Assert
        Assert.Equal(maxTitle, project.Title);
        Assert.Equal(Project.MaxTitleLength, project.Title.Length);
    }

    [Fact]
    public void Constructor_WithTitleExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var excessiveTitle = new string('T', Project.MaxTitleLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Project(tenantId, excessiveTitle));
        Assert.Equal("title", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceShortDescription_SetsNull(string? shortDescription)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var project = new Project(tenantId, "Title", shortDescription: shortDescription);

        // Assert
        Assert.Null(project.ShortDescription);
    }

    [Fact]
    public void Constructor_WithUntrimmedShortDescription_TrimsWhitespace()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var rawShortDescription = "   Short summary.   ";

        // Act
        var project = new Project(tenantId, "Title", shortDescription: rawShortDescription);

        // Assert
        Assert.Equal("Short summary.", project.ShortDescription);
    }

    [Fact]
    public void Constructor_WithShortDescriptionAtMaximumLength_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var maxShortDesc = new string('S', Project.MaxShortDescriptionLength);

        // Act
        var project = new Project(tenantId, "Title", shortDescription: maxShortDesc);

        // Assert
        Assert.NotNull(project.ShortDescription);
        Assert.Equal(maxShortDesc, project.ShortDescription);
        Assert.Equal(Project.MaxShortDescriptionLength, project.ShortDescription.Length);
    }

    [Fact]
    public void Constructor_WithShortDescriptionExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var excessiveShortDesc = new string('S', Project.MaxShortDescriptionLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Project(tenantId, "Title", shortDescription: excessiveShortDesc));
        Assert.Equal("shortDescription", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceDescription_SetsNull(string? description)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var project = new Project(tenantId, "Title", description: description);

        // Assert
        Assert.Null(project.Description);
    }

    [Fact]
    public void Constructor_WithUntrimmedDescription_TrimsWhitespace()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var rawDescription = "   Detailed description of the project.   ";

        // Act
        var project = new Project(tenantId, "Title", description: rawDescription);

        // Assert
        Assert.Equal("Detailed description of the project.", project.Description);
    }

    [Fact]
    public void Constructor_WithDescriptionAtMaximumLength_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var maxDesc = new string('D', Project.MaxDescriptionLength);

        // Act
        var project = new Project(tenantId, "Title", description: maxDesc);

        // Assert
        Assert.NotNull(project.Description);
        Assert.Equal(maxDesc, project.Description);
        Assert.Equal(Project.MaxDescriptionLength, project.Description.Length);
    }

    [Fact]
    public void Constructor_WithDescriptionExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var excessiveDesc = new string('D', Project.MaxDescriptionLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Project(tenantId, "Title", description: excessiveDesc));
        Assert.Equal("description", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidDisplayOrder_SetsDisplayOrder()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var project = new Project(tenantId, "Title", displayOrder: 5);

        // Assert
        Assert.Equal(5, project.DisplayOrder);
    }

    [Fact]
    public void Constructor_WithNegativeDisplayOrder_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Project(tenantId, "Title", displayOrder: -1));
        Assert.Equal("displayOrder", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullTechnologies_InitializesEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var project = new Project(tenantId, "Title", technologies: null);

        // Assert
        Assert.NotNull(project.Technologies);
        Assert.Empty(project.Technologies);
    }

    [Fact]
    public void Constructor_WithValidTechnologies_PreservesCallerOrderAndTrims()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var rawTechnologies = new[] { "  C#  ", " ASP.NET Core ", " PostgreSQL " };

        // Act
        var project = new Project(tenantId, "Title", technologies: rawTechnologies);

        // Assert
        Assert.Equal(3, project.Technologies.Count);
        Assert.Equal("C#", project.Technologies[0]);
        Assert.Equal("ASP.NET Core", project.Technologies[1]);
        Assert.Equal("PostgreSQL", project.Technologies[2]);
    }

    [Fact]
    public void Constructor_PreservesOriginalCasingOfTechnologies()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var rawTechnologies = new[] { "TypeScript", "Next.js", "Docker", "GraphQL" };

        // Act
        var project = new Project(tenantId, "Title", technologies: rawTechnologies);

        // Assert
        Assert.Equal(new[] { "TypeScript", "Next.js", "Docker", "GraphQL" }, project.Technologies);
    }

    [Fact]
    public void Constructor_WithNullTechnologyEntry_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var technologiesWithNull = new string?[] { "React", null, "Tailwind" };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Project(tenantId, "Title", technologies: technologiesWithNull!));
        Assert.Equal("technologies", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Constructor_WithEmptyOrWhitespaceTechnologyEntry_ThrowsArgumentException(string invalidEntry)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var technologies = new[] { "React", invalidEntry, "Tailwind" };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Project(tenantId, "Title", technologies: technologies));
        Assert.Equal("technologies", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithTechnologyAtMaximumLength_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var maxTech = new string('T', Project.MaxTechnologyLength);

        // Act
        var project = new Project(tenantId, "Title", technologies: [maxTech]);

        // Assert
        Assert.Single(project.Technologies);
        Assert.Equal(maxTech, project.Technologies[0]);
    }

    [Fact]
    public void Constructor_WithTechnologyExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var excessiveTech = new string('T', Project.MaxTechnologyLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Project(tenantId, "Title", technologies: [excessiveTech]));
        Assert.Equal("technologies", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithExactly20Technologies_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var twentyTechs = Enumerable.Range(1, Project.MaxTechnologiesCount).Select(i => $"Tech-{i}").ToList();

        // Act
        var project = new Project(tenantId, "Title", technologies: twentyTechs);

        // Assert
        Assert.Equal(Project.MaxTechnologiesCount, project.Technologies.Count);
    }

    [Fact]
    public void Constructor_WithMoreThan20Technologies_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var twentyOneTechs = Enumerable.Range(1, Project.MaxTechnologiesCount + 1).Select(i => $"Tech-{i}").ToList();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Project(tenantId, "Title", technologies: twentyOneTechs));
        Assert.Equal("technologies", exception.ParamName);
    }

    [Fact]
    public void Constructor_With21stItem_StopsEnumerationImmediatelyWithoutEvaluatingFurtherItems()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        static IEnumerable<string> InfiniteTechnologiesSequence()
        {
            for (var i = 1; i <= 21; i++)
            {
                yield return $"Tech-{i}";
            }

            throw new InvalidOperationException("Enumeration should have stopped at the 21st item!");
        }

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Project(tenantId, "Title", technologies: InfiniteTechnologiesSequence()));
        Assert.Equal("technologies", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithDuplicateTechnologiesIdenticalCasing_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var duplicates = new[] { "C#", "SQL", "C#" };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Project(tenantId, "Title", technologies: duplicates));
        Assert.Equal("technologies", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithDuplicateTechnologiesDifferentCasing_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var duplicates = new[] { "React", "TypeScript", "react" };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Project(tenantId, "Title", technologies: duplicates));
        Assert.Equal("technologies", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithDuplicateTechnologiesDifferentPadding_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var duplicates = new[] { "Docker", "  docker  " };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Project(tenantId, "Title", technologies: duplicates));
        Assert.Equal("technologies", exception.ParamName);
    }

    [Fact]
    public void Constructor_EncapsulatesTechnologies_CallerCannotMutateThroughCasting()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalList = new List<string> { "C#", "Docker" };
        var project = new Project(tenantId, "Title", technologies: originalList);

        // Act & Assert 1: Mutating the input collection has no effect
        originalList.Add("Kubernetes");
        Assert.Equal(2, project.Technologies.Count);

        // Act & Assert 2: Casting to List<string> fails
        Assert.Throws<InvalidCastException>(() => (List<string>)project.Technologies);

        // Act & Assert 3: Mutating via IList interface throws NotSupportedException
        var ilist = (IList<string>)project.Technologies;
        Assert.Throws<NotSupportedException>(() => ilist.Add("HackerTech"));
    }

    [Fact]
    public void UpdateDetails_WithAllNewValidValues_UpdatesAllPropertiesAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var project = new Project(tenantId, "Original Title", "Original Short", "Original Desc");
        var newGitHubUrl = new WebUrl("https://github.com/developer/updated");
        var newLiveUrl = new WebUrl("https://updated.dev/live");
        var newTechs = new[] { "Rust", "WebAssembly" };

        // Act
        project.UpdateDetails(
            "Updated Title",
            "Updated Short",
            "Updated Desc",
            newGitHubUrl,
            newLiveUrl,
            true,
            3,
            newTechs);

        // Assert
        Assert.Equal("Updated Title", project.Title);
        Assert.Equal("Updated Short", project.ShortDescription);
        Assert.Equal("Updated Desc", project.Description);
        Assert.Equal(newGitHubUrl, project.GitHubUrl);
        Assert.Equal(newLiveUrl, project.LiveUrl);
        Assert.True(project.IsFeatured);
        Assert.Equal(3, project.DisplayOrder);
        Assert.Equal(newTechs, project.Technologies);
        Assert.NotNull(project.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_ClearingOptionalFields_SetsOptionalFieldsToNullAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var project = new Project(
            tenantId,
            "Title",
            "Short",
            "Desc",
            new WebUrl("https://github.com/dev/repo"),
            new WebUrl("https://live.dev"),
            true,
            1,
            new[] { "C#" });

        // Act
        project.UpdateDetails(
            "Title",
            null,
            null,
            null,
            null,
            false,
            0,
            null);

        // Assert
        Assert.Null(project.ShortDescription);
        Assert.Null(project.Description);
        Assert.Null(project.GitHubUrl);
        Assert.Null(project.LiveUrl);
        Assert.False(project.IsFeatured);
        Assert.Equal(0, project.DisplayOrder);
        Assert.Empty(project.Technologies);
        Assert.NotNull(project.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_ReorderingTechnologies_CountsAsChangeAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var project = new Project(tenantId, "Title", technologies: ["A", "B", "C"]);

        // Act
        project.UpdateDetails(
            "Title",
            null,
            null,
            null,
            null,
            false,
            0,
            ["C", "B", "A"]);

        // Assert
        Assert.Equal(new[] { "C", "B", "A" }, project.Technologies);
        Assert.NotNull(project.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_ChangingTechnologyCasing_CountsAsChangeAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var project = new Project(tenantId, "Title", technologies: ["react"]);

        // Act
        project.UpdateDetails(
            "Title",
            null,
            null,
            null,
            null,
            false,
            0,
            ["React"]);

        // Assert
        Assert.Equal(new[] { "React" }, project.Technologies);
        Assert.NotNull(project.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithIdenticalValues_DoesNotModifyStateOrUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var gitHubUrl = new WebUrl("https://github.com/dev/repo");
        var liveUrl = new WebUrl("https://live.dev");
        var project = new Project(
            tenantId,
            "Title",
            "Short Description",
            "Long Description",
            gitHubUrl,
            liveUrl,
            true,
            5,
            ["C#", "PostgreSQL"]);

        // Act - supply same values with different surrounding whitespace that normalizes identically
        project.UpdateDetails(
            "  Title  ",
            "  Short Description  ",
            "  Long Description  ",
            new WebUrl("https://github.com/dev/repo"),
            new WebUrl("https://live.dev/"), // normalizes identically
            true,
            5,
            ["  C#  ", "  PostgreSQL  "]);

        // Assert
        Assert.Null(project.UpdatedAtUtc);
        Assert.Equal("Title", project.Title);
        Assert.Equal("Short Description", project.ShortDescription);
        Assert.Equal("Long Description", project.Description);
        Assert.Equal(5, project.DisplayOrder);
        Assert.True(project.IsFeatured);
        Assert.Equal(new[] { "C#", "PostgreSQL" }, project.Technologies);
    }

    [Fact]
    public void UpdateDetails_PreservesIdAndTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var project = new Project(tenantId, "Original");
        var originalId = project.Id;
        var originalTenantId = project.TenantId;

        // Act
        project.UpdateDetails("Updated", null, null, null, null, false, 0, null);

        // Assert
        Assert.Equal(originalId, project.Id);
        Assert.Equal(originalTenantId, project.TenantId);
    }

    // =========================================================================
    // ATOMICITY & REGRESSION HARDENING TESTS
    // =========================================================================

    [Fact]
    public void UpdateDetails_WhenTitleInvalid_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalGitHub = new WebUrl("https://github.com/dev/original");
        var project = new Project(
            tenantId,
            "Original Title",
            "Original Short",
            "Original Desc",
            originalGitHub,
            null,
            true,
            1,
            ["C#", "Docker"]);

        var originalId = project.Id;
        var originalTenantId = project.TenantId;
        var originalUpdatedAt = project.UpdatedAtUtc;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => project.UpdateDetails(
            "   ",
            "New Short",
            "New Desc",
            new WebUrl("https://github.com/dev/new"),
            null,
            false,
            2,
            ["Rust"]));

        Assert.Equal("title", exception.ParamName);
        Assert.Equal("Original Title", project.Title);
        Assert.Equal("Original Short", project.ShortDescription);
        Assert.Equal("Original Desc", project.Description);
        Assert.Equal(originalGitHub, project.GitHubUrl);
        Assert.Null(project.LiveUrl);
        Assert.True(project.IsFeatured);
        Assert.Equal(1, project.DisplayOrder);
        Assert.Equal(new[] { "C#", "Docker" }, project.Technologies);
        Assert.Equal(originalId, project.Id);
        Assert.Equal(originalTenantId, project.TenantId);
        Assert.Equal(originalUpdatedAt, project.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenShortDescriptionExceedsMaxLength_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var project = new Project(tenantId, "Original Title", "Original Short");
        var originalId = project.Id;
        var originalTenantId = project.TenantId;
        var originalUpdatedAt = project.UpdatedAtUtc;
        var longShortDesc = new string('S', Project.MaxShortDescriptionLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => project.UpdateDetails(
            "New Valid Title",
            longShortDesc,
            "New Desc",
            null,
            null,
            false,
            0,
            null));

        Assert.Equal("shortDescription", exception.ParamName);
        Assert.Equal("Original Title", project.Title);
        Assert.Equal("Original Short", project.ShortDescription);
        Assert.Equal(originalId, project.Id);
        Assert.Equal(originalTenantId, project.TenantId);
        Assert.Equal(originalUpdatedAt, project.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenDescriptionExceedsMaxLength_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var project = new Project(tenantId, "Original Title", description: "Original Desc");
        var originalId = project.Id;
        var originalTenantId = project.TenantId;
        var originalUpdatedAt = project.UpdatedAtUtc;
        var longDesc = new string('D', Project.MaxDescriptionLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => project.UpdateDetails(
            "New Valid Title",
            null,
            longDesc,
            null,
            null,
            false,
            0,
            null));

        Assert.Equal("description", exception.ParamName);
        Assert.Equal("Original Title", project.Title);
        Assert.Equal("Original Desc", project.Description);
        Assert.Equal(originalId, project.Id);
        Assert.Equal(originalTenantId, project.TenantId);
        Assert.Equal(originalUpdatedAt, project.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenDisplayOrderNegative_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var project = new Project(tenantId, "Original Title", displayOrder: 2);
        var originalId = project.Id;
        var originalTenantId = project.TenantId;
        var originalUpdatedAt = project.UpdatedAtUtc;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => project.UpdateDetails(
            "New Valid Title",
            null,
            null,
            null,
            null,
            false,
            -5,
            null));

        Assert.Equal("displayOrder", exception.ParamName);
        Assert.Equal("Original Title", project.Title);
        Assert.Equal(2, project.DisplayOrder);
        Assert.Equal(originalId, project.Id);
        Assert.Equal(originalTenantId, project.TenantId);
        Assert.Equal(originalUpdatedAt, project.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenTechnologyContainsNull_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var project = new Project(tenantId, "Original Title", technologies: ["C#", "Docker"]);
        var originalId = project.Id;
        var originalTenantId = project.TenantId;
        var originalUpdatedAt = project.UpdatedAtUtc;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => project.UpdateDetails(
            "New Valid Title",
            null,
            null,
            null,
            null,
            false,
            0,
            new string?[] { "Rust", null, "Go" }!));

        Assert.Equal("technologies", exception.ParamName);
        Assert.Equal("Original Title", project.Title);
        Assert.Equal(new[] { "C#", "Docker" }, project.Technologies);
        Assert.Equal(originalId, project.Id);
        Assert.Equal(originalTenantId, project.TenantId);
        Assert.Equal(originalUpdatedAt, project.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenTechnologyContainsEmptyOrWhitespace_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var project = new Project(tenantId, "Original Title", technologies: ["C#", "Docker"]);
        var originalId = project.Id;
        var originalTenantId = project.TenantId;
        var originalUpdatedAt = project.UpdatedAtUtc;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => project.UpdateDetails(
            "New Valid Title",
            null,
            null,
            null,
            null,
            false,
            0,
            new[] { "Rust", "   ", "Go" }));

        Assert.Equal("technologies", exception.ParamName);
        Assert.Equal("Original Title", project.Title);
        Assert.Equal(new[] { "C#", "Docker" }, project.Technologies);
        Assert.Equal(originalId, project.Id);
        Assert.Equal(originalTenantId, project.TenantId);
        Assert.Equal(originalUpdatedAt, project.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenTechnologyExceedsMaxLength_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var project = new Project(tenantId, "Original Title", technologies: ["C#", "Docker"]);
        var originalId = project.Id;
        var originalTenantId = project.TenantId;
        var originalUpdatedAt = project.UpdatedAtUtc;
        var excessiveTech = new string('T', Project.MaxTechnologyLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => project.UpdateDetails(
            "New Valid Title",
            null,
            null,
            null,
            null,
            false,
            0,
            new[] { excessiveTech }));

        Assert.Equal("technologies", exception.ParamName);
        Assert.Equal("Original Title", project.Title);
        Assert.Equal(new[] { "C#", "Docker" }, project.Technologies);
        Assert.Equal(originalId, project.Id);
        Assert.Equal(originalTenantId, project.TenantId);
        Assert.Equal(originalUpdatedAt, project.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenTechnologiesContainDuplicates_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var project = new Project(tenantId, "Original Title", technologies: ["C#", "Docker"]);
        var originalId = project.Id;
        var originalTenantId = project.TenantId;
        var originalUpdatedAt = project.UpdatedAtUtc;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => project.UpdateDetails(
            "New Valid Title",
            null,
            null,
            null,
            null,
            false,
            0,
            new[] { "React", "TypeScript", "REACT" }));

        Assert.Equal("technologies", exception.ParamName);
        Assert.Equal("Original Title", project.Title);
        Assert.Equal(new[] { "C#", "Docker" }, project.Technologies);
        Assert.Equal(originalId, project.Id);
        Assert.Equal(originalTenantId, project.TenantId);
        Assert.Equal(originalUpdatedAt, project.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenTechnologiesExceedMaximumCount_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var project = new Project(tenantId, "Original Title", technologies: ["C#", "Docker"]);
        var originalId = project.Id;
        var originalTenantId = project.TenantId;
        var originalUpdatedAt = project.UpdatedAtUtc;
        var twentyOneTechs = Enumerable.Range(1, Project.MaxTechnologiesCount + 1).Select(i => $"Tech-{i}").ToList();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => project.UpdateDetails(
            "New Valid Title",
            null,
            null,
            null,
            null,
            false,
            0,
            twentyOneTechs));

        Assert.Equal("technologies", exception.ParamName);
        Assert.Equal("Original Title", project.Title);
        Assert.Equal(new[] { "C#", "Docker" }, project.Technologies);
        Assert.Equal(originalId, project.Id);
        Assert.Equal(originalTenantId, project.TenantId);
        Assert.Equal(originalUpdatedAt, project.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenTechnologyEnumerationThrows_PreservesAllEntityState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalGitHub = new WebUrl("https://github.com/dev/original");
        var originalLive = new WebUrl("https://original.dev/live");
        var project = new Project(
            tenantId,
            "Original Title",
            "Original Short",
            "Original Desc",
            originalGitHub,
            originalLive,
            true,
            1,
            ["C#", "Docker"]);

        var originalId = project.Id;
        var originalTenantId = project.TenantId;
        var originalTitle = project.Title;
        var originalShortDescription = project.ShortDescription;
        var originalDescription = project.Description;
        var originalGitHubUrl = project.GitHubUrl;
        var originalLiveUrl = project.LiveUrl;
        var originalIsFeatured = project.IsFeatured;
        var originalDisplayOrder = project.DisplayOrder;
        var originalTechnologies = project.Technologies.ToList();
        var originalUpdatedAt = project.UpdatedAtUtc;

        static IEnumerable<string> ThrowingSequence()
        {
            yield return "Rust";
            throw new InvalidOperationException("Deferred sequence evaluation failed!");
        }

        var newGitHub = new WebUrl("https://github.com/dev/new");
        var newLive = new WebUrl("https://new.dev/live");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => project.UpdateDetails(
            "New Valid Title",
            "New Valid Short",
            "New Valid Description",
            newGitHub,
            newLive,
            false,
            5,
            ThrowingSequence()));

        Assert.Equal("Deferred sequence evaluation failed!", exception.Message);
        Assert.Equal(originalId, project.Id);
        Assert.Equal(originalTenantId, project.TenantId);
        Assert.Equal(originalTitle, project.Title);
        Assert.Equal(originalShortDescription, project.ShortDescription);
        Assert.Equal(originalDescription, project.Description);
        Assert.Equal(originalGitHubUrl, project.GitHubUrl);
        Assert.Equal(originalLiveUrl, project.LiveUrl);
        Assert.Equal(originalIsFeatured, project.IsFeatured);
        Assert.Equal(originalDisplayOrder, project.DisplayOrder);
        Assert.Equal(originalTechnologies, project.Technologies);
        Assert.Equal(originalUpdatedAt, project.UpdatedAtUtc);
    }
}
