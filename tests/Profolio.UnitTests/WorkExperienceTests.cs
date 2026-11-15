using Profolio.Domain.Common;
using Profolio.Domain.Entities;

namespace Profolio.UnitTests;

public class WorkExperienceTests
{
    [Fact]
    public void Constructor_WithValidCompletedEmploymentRecord_CreatesWorkExperience()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var companyName = "Acme Corp";
        var role = "Senior Engineer";
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = new DateOnly(2023, 1, 1);
        var isCurrent = false;
        var location = "San Francisco, CA";
        var description = "Built distributed systems.";
        var displayOrder = 1;

        // Act
        var experience = new WorkExperience(
            tenantId,
            companyName,
            role,
            startDate,
            endDate,
            isCurrent,
            location,
            description,
            displayOrder);

        // Assert
        Assert.NotEqual(Guid.Empty, experience.Id);
        Assert.Equal(tenantId, experience.TenantId);
        Assert.Equal("Acme Corp", experience.CompanyName);
        Assert.Equal("Senior Engineer", experience.Role);
        Assert.Equal(startDate, experience.StartDate);
        Assert.Equal(endDate, experience.EndDate);
        Assert.False(experience.IsCurrent);
        Assert.Equal("San Francisco, CA", experience.Location);
        Assert.Equal("Built distributed systems.", experience.Description);
        Assert.Equal(1, experience.DisplayOrder);
        Assert.True(experience.CreatedAtUtc <= DateTime.UtcNow);
        Assert.Null(experience.UpdatedAtUtc);
        Assert.IsAssignableFrom<ITenantEntity>(experience);
        Assert.IsAssignableFrom<BaseEntity>(experience);
    }

    [Fact]
    public void Constructor_WithValidCurrentEmploymentRecord_CreatesWorkExperience()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var startDate = new DateOnly(2023, 2, 1);

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Tech Innovations",
            "Tech Lead",
            startDate,
            endDate: null,
            isCurrent: true);

        // Assert
        Assert.Equal("Tech Innovations", experience.CompanyName);
        Assert.Equal("Tech Lead", experience.Role);
        Assert.Equal(startDate, experience.StartDate);
        Assert.Null(experience.EndDate);
        Assert.True(experience.IsCurrent);
        Assert.Null(experience.Location);
        Assert.Null(experience.Description);
        Assert.Equal(0, experience.DisplayOrder);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WorkExperience(
            emptyTenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true));

        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullCompanyName_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new WorkExperience(
            tenantId,
            null!,
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true));

        Assert.Equal("companyName", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithEmptyOrWhitespaceCompanyName_ThrowsArgumentException(string invalidCompanyName)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WorkExperience(
            tenantId,
            invalidCompanyName,
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true));

        Assert.Equal("companyName", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithCompanyNameSurroundedByWhitespace_TrimsCompanyName()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var experience = new WorkExperience(
            tenantId,
            "  Acme Corp  ",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true);

        // Assert
        Assert.Equal("Acme Corp", experience.CompanyName);
    }

    [Fact]
    public void Constructor_WithCompanyNameAtMaximumLength_AcceptsCompanyName()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var companyName = new string('A', WorkExperience.MaxCompanyNameLength);

        // Act
        var experience = new WorkExperience(
            tenantId,
            companyName,
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true);

        // Assert
        Assert.Equal(WorkExperience.MaxCompanyNameLength, experience.CompanyName.Length);
        Assert.Equal(companyName, experience.CompanyName);
    }

    [Fact]
    public void Constructor_WithCompanyNameExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var longCompanyName = new string('A', WorkExperience.MaxCompanyNameLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WorkExperience(
            tenantId,
            longCompanyName,
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true));

        Assert.Equal("companyName", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullRole_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new WorkExperience(
            tenantId,
            "Acme",
            null!,
            new DateOnly(2022, 1, 1),
            null,
            true));

        Assert.Equal("role", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithEmptyOrWhitespaceRole_ThrowsArgumentException(string invalidRole)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WorkExperience(
            tenantId,
            "Acme",
            invalidRole,
            new DateOnly(2022, 1, 1),
            null,
            true));

        Assert.Equal("role", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithRoleSurroundedByWhitespace_TrimsRole()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "  Principal Architect  ",
            new DateOnly(2022, 1, 1),
            null,
            true);

        // Assert
        Assert.Equal("Principal Architect", experience.Role);
    }

    [Fact]
    public void Constructor_WithRoleAtMaximumLength_AcceptsRole()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var role = new string('R', WorkExperience.MaxRoleLength);

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            role,
            new DateOnly(2022, 1, 1),
            null,
            true);

        // Assert
        Assert.Equal(WorkExperience.MaxRoleLength, experience.Role.Length);
        Assert.Equal(role, experience.Role);
    }

    [Fact]
    public void Constructor_WithRoleExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var longRole = new string('R', WorkExperience.MaxRoleLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WorkExperience(
            tenantId,
            "Acme",
            longRole,
            new DateOnly(2022, 1, 1),
            null,
            true));

        Assert.Equal("role", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLocation_SetsLocationToNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null);

        // Assert
        Assert.Null(experience.Location);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Constructor_WithEmptyOrWhitespaceLocation_NormalizesLocationToNull(string invalidLocation)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: invalidLocation);

        // Assert
        Assert.Null(experience.Location);
    }

    [Fact]
    public void Constructor_WithLocationSurroundedByWhitespace_TrimsLocation()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: "  Austin, TX  ");

        // Assert
        Assert.Equal("Austin, TX", experience.Location);
    }

    [Fact]
    public void Constructor_WithLocationAtMaximumLength_AcceptsLocation()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var location = new string('L', WorkExperience.MaxLocationLength);

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: location);

        // Assert
        Assert.Equal(WorkExperience.MaxLocationLength, experience.Location!.Length);
        Assert.Equal(location, experience.Location);
    }

    [Fact]
    public void Constructor_WithLocationExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var longLocation = new string('L', WorkExperience.MaxLocationLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: longLocation));

        Assert.Equal("location", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullDescription_SetsDescriptionToNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            description: null);

        // Assert
        Assert.Null(experience.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Constructor_WithEmptyOrWhitespaceDescription_NormalizesDescriptionToNull(string invalidDescription)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            description: invalidDescription);

        // Assert
        Assert.Null(experience.Description);
    }

    [Fact]
    public void Constructor_WithDescriptionSurroundedByWhitespace_TrimsDescription()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            description: "  Led cloud migration.  ");

        // Assert
        Assert.Equal("Led cloud migration.", experience.Description);
    }

    [Fact]
    public void Constructor_WithDescriptionAtMaximumLength_AcceptsDescription()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var description = new string('D', WorkExperience.MaxDescriptionLength);

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            description: description);

        // Assert
        Assert.Equal(WorkExperience.MaxDescriptionLength, experience.Description!.Length);
        Assert.Equal(description, experience.Description);
    }

    [Fact]
    public void Constructor_WithDescriptionExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var longDescription = new string('D', WorkExperience.MaxDescriptionLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            description: longDescription));

        Assert.Equal("description", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEndDateEqualToStartDate_AcceptsDates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var sameDate = new DateOnly(2023, 5, 1);

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Contractor",
            sameDate,
            sameDate,
            isCurrent: false);

        // Assert
        Assert.Equal(sameDate, experience.StartDate);
        Assert.Equal(sameDate, experience.EndDate);
    }

    [Fact]
    public void Constructor_WithEndDateAfterStartDate_AcceptsDates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = new DateOnly(2023, 6, 30);

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            startDate,
            endDate,
            isCurrent: false);

        // Assert
        Assert.Equal(startDate, experience.StartDate);
        Assert.Equal(endDate, experience.EndDate);
    }

    [Fact]
    public void Constructor_WithEndDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var startDate = new DateOnly(2023, 6, 1);
        var endDate = new DateOnly(2023, 1, 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            startDate,
            endDate,
            isCurrent: false));

        Assert.Equal("endDate", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithIsCurrentTrueAndNullEndDate_AcceptsDates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var startDate = new DateOnly(2023, 1, 1);

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            startDate,
            endDate: null,
            isCurrent: true);

        // Assert
        Assert.True(experience.IsCurrent);
        Assert.Null(experience.EndDate);
    }

    [Fact]
    public void Constructor_WithIsCurrentTrueAndNonNullEndDate_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var startDate = new DateOnly(2022, 1, 1);
        var endDate = new DateOnly(2023, 1, 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            startDate,
            endDate,
            isCurrent: true));

        Assert.Equal("endDate", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithIsCurrentFalseAndNullEndDate_AcceptsDates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var startDate = new DateOnly(2022, 1, 1);

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            startDate,
            endDate: null,
            isCurrent: false);

        // Assert
        Assert.False(experience.IsCurrent);
        Assert.Null(experience.EndDate);
    }

    [Fact]
    public void Constructor_WithFutureStartDate_AcceptsDates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var futureStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2));

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Future Corp",
            "Lead",
            futureStartDate,
            endDate: null,
            isCurrent: true);

        // Assert
        Assert.Equal(futureStartDate, experience.StartDate);
    }

    [Fact]
    public void Constructor_WithDateOnlyMinValueStartDate_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            DateOnly.MinValue,
            endDate: null,
            isCurrent: true));

        Assert.Equal("startDate", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNegativeDisplayOrder_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            displayOrder: -1));

        Assert.Equal("displayOrder", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(42)]
    public void Constructor_WithZeroOrPositiveDisplayOrder_AcceptsDisplayOrder(int displayOrder)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            displayOrder: displayOrder);

        // Assert
        Assert.Equal(displayOrder, experience.DisplayOrder);
    }

    [Fact]
    public void Constructor_WithValidTenantId_PreservesTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true);

        // Assert
        Assert.Equal(tenantId, experience.TenantId);
    }

    [Fact]
    public void UpdateDetails_WithAllPropertiesChanged_UpdatesAllPropertiesAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Old Corp",
            "Junior Dev",
            new DateOnly(2020, 1, 1),
            new DateOnly(2021, 1, 1),
            isCurrent: false,
            location: "Old City",
            description: "Old description",
            displayOrder: 0);

        var beforeUpdate = DateTime.UtcNow;

        // Act
        experience.UpdateDetails(
            "New Corp",
            "Senior Architect",
            new DateOnly(2021, 2, 1),
            null,
            isCurrent: true,
            location: "New City",
            description: "New description",
            displayOrder: 5);

        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.Equal("New Corp", experience.CompanyName);
        Assert.Equal("Senior Architect", experience.Role);
        Assert.Equal(new DateOnly(2021, 2, 1), experience.StartDate);
        Assert.Null(experience.EndDate);
        Assert.True(experience.IsCurrent);
        Assert.Equal("New City", experience.Location);
        Assert.Equal("New description", experience.Description);
        Assert.Equal(5, experience.DisplayOrder);
        Assert.NotNull(experience.UpdatedAtUtc);
        Assert.InRange(experience.UpdatedAtUtc.Value, beforeUpdate, afterUpdate);
    }

    [Fact]
    public void UpdateDetails_WithCompanyNameOnlyChanged_UpdatesCompanyNameAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Old Corp",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true);

        // Act
        experience.UpdateDetails(
            "Updated Corp",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null,
            description: null,
            displayOrder: 0);

        // Assert
        Assert.Equal("Updated Corp", experience.CompanyName);
        Assert.NotNull(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithRoleOnlyChanged_UpdatesRoleAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Old Role",
            new DateOnly(2022, 1, 1),
            null,
            true);

        // Act
        experience.UpdateDetails(
            "Acme",
            "New Role",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null,
            description: null,
            displayOrder: 0);

        // Assert
        Assert.Equal("New Role", experience.Role);
        Assert.NotNull(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithLocationOnlyChanged_UpdatesLocationAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: "Old Location");

        // Act
        experience.UpdateDetails(
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: "New Location",
            description: null,
            displayOrder: 0);

        // Assert
        Assert.Equal("New Location", experience.Location);
        Assert.NotNull(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithDatesOnlyChanged_UpdatesDatesAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            new DateOnly(2022, 12, 31),
            isCurrent: false);

        var newStartDate = new DateOnly(2021, 6, 1);
        var newEndDate = new DateOnly(2023, 6, 1);

        // Act
        experience.UpdateDetails(
            "Acme",
            "Dev",
            newStartDate,
            newEndDate,
            isCurrent: false,
            location: null,
            description: null,
            displayOrder: 0);

        // Assert
        Assert.Equal(newStartDate, experience.StartDate);
        Assert.Equal(newEndDate, experience.EndDate);
        Assert.NotNull(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithDescriptionOnlyChanged_UpdatesDescriptionAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            description: "Old text");

        // Act
        experience.UpdateDetails(
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null,
            description: "New text",
            displayOrder: 0);

        // Assert
        Assert.Equal("New text", experience.Description);
        Assert.NotNull(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithDisplayOrderOnlyChanged_UpdatesDisplayOrderAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            displayOrder: 0);

        // Act
        experience.UpdateDetails(
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null,
            description: null,
            displayOrder: 10);

        // Assert
        Assert.Equal(10, experience.DisplayOrder);
        Assert.NotNull(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithInvalidCompanyName_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => experience.UpdateDetails(
            "",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null,
            description: null,
            displayOrder: 0));

        Assert.Equal("companyName", exception.ParamName);
        Assert.Equal("Acme", experience.CompanyName);
        Assert.Null(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithInvalidRole_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => experience.UpdateDetails(
            "Acme",
            "   ",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null,
            description: null,
            displayOrder: 0));

        Assert.Equal("role", exception.ParamName);
        Assert.Equal("Dev", experience.Role);
        Assert.Null(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithInvalidLocation_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true);

        var longLocation = new string('L', WorkExperience.MaxLocationLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => experience.UpdateDetails(
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: longLocation,
            description: null,
            displayOrder: 0));

        Assert.Equal("location", exception.ParamName);
        Assert.Null(experience.Location);
        Assert.Null(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithInvalidDescription_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true);

        var longDescription = new string('D', WorkExperience.MaxDescriptionLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => experience.UpdateDetails(
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null,
            description: longDescription,
            displayOrder: 0));

        Assert.Equal("description", exception.ParamName);
        Assert.Null(experience.Description);
        Assert.Null(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithInvalidDateRange_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            new DateOnly(2022, 12, 31),
            isCurrent: false);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => experience.UpdateDetails(
            "Acme",
            "Dev",
            new DateOnly(2023, 6, 1),
            new DateOnly(2023, 1, 1),
            isCurrent: false,
            location: null,
            description: null,
            displayOrder: 0));

        Assert.Equal("endDate", exception.ParamName);
        Assert.Equal(new DateOnly(2022, 1, 1), experience.StartDate);
        Assert.Equal(new DateOnly(2022, 12, 31), experience.EndDate);
        Assert.Null(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithIsCurrentAndNonNullEndDate_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => experience.UpdateDetails(
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            new DateOnly(2023, 1, 1),
            isCurrent: true,
            location: null,
            description: null,
            displayOrder: 0));

        Assert.Equal("endDate", exception.ParamName);
        Assert.True(experience.IsCurrent);
        Assert.Null(experience.EndDate);
        Assert.Null(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithNegativeDisplayOrder_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            displayOrder: 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => experience.UpdateDetails(
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null,
            description: null,
            displayOrder: -1));

        Assert.Equal("displayOrder", exception.ParamName);
        Assert.Equal(1, experience.DisplayOrder);
        Assert.Null(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithIdenticalNormalizedValues_WhenInitiallyNull_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            new DateOnly(2023, 1, 1),
            isCurrent: false,
            location: "Boston",
            description: "Architect",
            displayOrder: 2);

        Assert.Null(experience.UpdatedAtUtc);

        // Act - reapply identical values
        experience.UpdateDetails(
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            new DateOnly(2023, 1, 1),
            isCurrent: false,
            location: "Boston",
            description: "Architect",
            displayOrder: 2);

        // Assert
        Assert.Null(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithIdenticalNormalizedValues_WhenAlreadyUpdated_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true);

        experience.UpdateDetails(
            "New Acme",
            "Tech Lead",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null,
            description: null,
            displayOrder: 1);

        var originalUpdatedAt = experience.UpdatedAtUtc;
        Assert.NotNull(originalUpdatedAt);

        // Act - reapply identical values
        experience.UpdateDetails(
            "New Acme",
            "Tech Lead",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null,
            description: null,
            displayOrder: 1);

        // Assert
        Assert.Equal(originalUpdatedAt, experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithDifferentStringInstancesHavingSameNormalizedValues_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme Corp",
            "Architect",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: "Remote",
            description: "Built cloud infra",
            displayOrder: 1);

        experience.UpdateDetails(
            "Acme Cloud",
            "Staff Architect",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: "Remote",
            description: "Built cloud infra",
            displayOrder: 1);

        var originalUpdatedAt = experience.UpdatedAtUtc;
        Assert.NotNull(originalUpdatedAt);

        // Act - reapply equivalent normalized values with extra padding
        experience.UpdateDetails(
            "  Acme Cloud  ",
            "  Staff Architect  ",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: "  Remote  ",
            description: "  Built cloud infra  ",
            displayOrder: 1);

        // Assert
        Assert.Equal(originalUpdatedAt, experience.UpdatedAtUtc);
        Assert.Equal("Acme Cloud", experience.CompanyName);
        Assert.Equal("Staff Architect", experience.Role);
        Assert.Equal("Remote", experience.Location);
        Assert.Equal("Built cloud infra", experience.Description);
    }

    [Fact]
    public void UpdateDetails_WithActualChanges_UpdatesUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true);

        Assert.Null(experience.UpdatedAtUtc);
        var before = DateTime.UtcNow;

        // Act
        experience.UpdateDetails(
            "Acme",
            "Senior Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null,
            description: null,
            displayOrder: 0);

        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(experience.UpdatedAtUtc);
        Assert.InRange(experience.UpdatedAtUtc.Value, before, after);
    }

    [Fact]
    public void UpdateDetails_WhenCalled_DoesNotModifyId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true);

        var originalId = experience.Id;

        // Act
        experience.UpdateDetails(
            "New Acme",
            "Senior Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null,
            description: null,
            displayOrder: 1);

        // Assert
        Assert.Equal(originalId, experience.Id);
    }

    [Fact]
    public void UpdateDetails_WhenCalled_DoesNotModifyTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true);

        var originalTenantId = experience.TenantId;

        // Act
        experience.UpdateDetails(
            "New Acme",
            "Senior Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: null,
            description: null,
            displayOrder: 1);

        // Assert
        Assert.Equal(originalTenantId, experience.TenantId);
    }

    [Fact]
    public void UpdateDetails_WithNullOrWhitespaceOptionalStrings_ClearsLocationAndDescription()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: "Seattle, WA",
            description: "Developed APIs");

        Assert.Equal("Seattle, WA", experience.Location);
        Assert.Equal("Developed APIs", experience.Description);

        // Act - clear with empty or whitespace
        experience.UpdateDetails(
            "Acme",
            "Dev",
            new DateOnly(2022, 1, 1),
            null,
            true,
            location: "   ",
            description: null,
            displayOrder: 0);

        // Assert
        Assert.Null(experience.Location);
        Assert.Null(experience.Description);
        Assert.NotNull(experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithInvalidMultiPropertyUpdate_DoesNotPartiallyMutateState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalStartDate = new DateOnly(2021, 1, 1);
        var originalEndDate = new DateOnly(2022, 1, 1);
        var experience = new WorkExperience(
            tenantId,
            "Original Corp",
            "Junior Dev",
            originalStartDate,
            originalEndDate,
            isCurrent: false,
            location: "Denver",
            description: "Original description",
            displayOrder: 1);

        var originalId = experience.Id;
        var originalTenantId = experience.TenantId;
        var originalUpdatedAt = experience.UpdatedAtUtc;
        var invalidLongDescription = new string('X', WorkExperience.MaxDescriptionLength + 1);

        // Act & Assert - CompanyName, Role, Location, StartDate are changed to valid new values, but Description exceeds max length
        var exception = Assert.Throws<ArgumentException>(() => experience.UpdateDetails(
            "Brand New Corp",
            "Staff Engineer",
            new DateOnly(2022, 2, 1),
            null,
            isCurrent: true,
            location: "Remote Everywhere",
            description: invalidLongDescription,
            displayOrder: 5));

        Assert.Equal("description", exception.ParamName);
        Assert.Equal("Original Corp", experience.CompanyName);
        Assert.Equal("Junior Dev", experience.Role);
        Assert.Equal(originalStartDate, experience.StartDate);
        Assert.Equal(originalEndDate, experience.EndDate);
        Assert.False(experience.IsCurrent);
        Assert.Equal("Denver", experience.Location);
        Assert.Equal("Original description", experience.Description);
        Assert.Equal(1, experience.DisplayOrder);
        Assert.Equal(originalId, experience.Id);
        Assert.Equal(originalTenantId, experience.TenantId);
        Assert.Equal(originalUpdatedAt, experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenStartDateValidButEndDateEarlierThanStartDate_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var experience = new WorkExperience(
            tenantId,
            "Original Corp",
            "Dev",
            new DateOnly(2021, 1, 1),
            new DateOnly(2022, 1, 1),
            isCurrent: false);

        var originalUpdatedAt = experience.UpdatedAtUtc;

        // Act & Assert - pass new company name and role, but invalid dates
        var exception = Assert.Throws<ArgumentException>(() => experience.UpdateDetails(
            "Changed Corp",
            "Changed Role",
            new DateOnly(2023, 6, 1),
            new DateOnly(2023, 1, 1),
            isCurrent: false,
            location: null,
            description: null,
            displayOrder: 0));

        Assert.Equal("endDate", exception.ParamName);
        Assert.Equal("Original Corp", experience.CompanyName);
        Assert.Equal("Dev", experience.Role);
        Assert.Equal(new DateOnly(2021, 1, 1), experience.StartDate);
        Assert.Equal(new DateOnly(2022, 1, 1), experience.EndDate);
        Assert.Equal(originalUpdatedAt, experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithDateOnlyMinValueStartDate_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalStartDate = new DateOnly(2022, 1, 1);
        var originalEndDate = new DateOnly(2023, 1, 1);
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Developer",
            originalStartDate,
            originalEndDate,
            isCurrent: false,
            location: "Pune",
            description: "Backend development",
            displayOrder: 1);

        var originalId = experience.Id;
        var originalTenantId = experience.TenantId;
        var originalUpdatedAt = experience.UpdatedAtUtc;

        // Act & Assert - pass DateOnly.MinValue as StartDate with otherwise valid changed values
        var exception = Assert.Throws<ArgumentException>(() => experience.UpdateDetails(
            "New Acme",
            "Senior Developer",
            DateOnly.MinValue,
            null,
            isCurrent: true,
            location: "Mumbai",
            description: "New description",
            displayOrder: 2));

        Assert.Equal("startDate", exception.ParamName);
        Assert.Equal("Acme", experience.CompanyName);
        Assert.Equal("Developer", experience.Role);
        Assert.Equal(originalStartDate, experience.StartDate);
        Assert.Equal(originalEndDate, experience.EndDate);
        Assert.False(experience.IsCurrent);
        Assert.Equal("Pune", experience.Location);
        Assert.Equal("Backend development", experience.Description);
        Assert.Equal(1, experience.DisplayOrder);
        Assert.Equal(originalId, experience.Id);
        Assert.Equal(originalTenantId, experience.TenantId);
        Assert.Equal(originalUpdatedAt, experience.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenNameValidButIsCurrentAndEndDateInvalid_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalStartDate = new DateOnly(2024, 1, 1);
        var originalEndDate = new DateOnly(2024, 12, 31);
        var experience = new WorkExperience(
            tenantId,
            "Acme",
            "Developer",
            originalStartDate,
            originalEndDate,
            isCurrent: false,
            location: "Pune",
            description: "Original description",
            displayOrder: 1);

        var originalId = experience.Id;
        var originalTenantId = experience.TenantId;
        var originalUpdatedAt = experience.UpdatedAtUtc;

        // Act & Assert - pass changed company name, role, but isCurrent = true with non-null EndDate
        var exception = Assert.Throws<ArgumentException>(() => experience.UpdateDetails(
            "New Company",
            "Senior Developer",
            new DateOnly(2024, 1, 1),
            new DateOnly(2025, 1, 1),
            isCurrent: true,
            location: "Mumbai",
            description: "Updated description",
            displayOrder: 2));

        Assert.Equal("endDate", exception.ParamName);
        Assert.Equal("Acme", experience.CompanyName);
        Assert.Equal("Developer", experience.Role);
        Assert.Equal(originalStartDate, experience.StartDate);
        Assert.Equal(originalEndDate, experience.EndDate);
        Assert.False(experience.IsCurrent);
        Assert.Equal("Pune", experience.Location);
        Assert.Equal("Original description", experience.Description);
        Assert.Equal(1, experience.DisplayOrder);
        Assert.Equal(originalId, experience.Id);
        Assert.Equal(originalTenantId, experience.TenantId);
        Assert.Equal(originalUpdatedAt, experience.UpdatedAtUtc);
    }
}
