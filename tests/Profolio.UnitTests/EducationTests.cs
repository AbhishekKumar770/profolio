using Profolio.Domain.Common;
using Profolio.Domain.Entities;

namespace Profolio.UnitTests;

public class EducationTests
{
    [Fact]
    public void Constructor_WithValidArguments_CreatesEducation()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var institution = "Massachusetts Institute of Technology";
        var startDate = new DateOnly(2018, 9, 1);
        var endDate = new DateOnly(2022, 6, 1);
        var isCurrent = false;
        var degree = "Bachelor of Science";
        var fieldOfStudy = "Computer Science";
        var location = "Cambridge, MA";
        var description = "Graduated with honors.";
        var displayOrder = 1;

        // Act
        var education = new Education(
            tenantId,
            institution,
            startDate,
            endDate,
            isCurrent,
            degree,
            fieldOfStudy,
            location,
            description,
            displayOrder);

        // Assert
        Assert.NotEqual(Guid.Empty, education.Id);
        Assert.Equal(tenantId, education.TenantId);
        Assert.Equal(institution, education.Institution);
        Assert.Equal(startDate, education.StartDate);
        Assert.Equal(endDate, education.EndDate);
        Assert.False(education.IsCurrent);
        Assert.Equal(degree, education.Degree);
        Assert.Equal(fieldOfStudy, education.FieldOfStudy);
        Assert.Equal(location, education.Location);
        Assert.Equal(description, education.Description);
        Assert.Equal(1, education.DisplayOrder);
        Assert.True(education.CreatedAtUtc <= DateTime.UtcNow);
        Assert.Null(education.UpdatedAtUtc);
        Assert.IsAssignableFrom<ITenantEntity>(education);
        Assert.IsAssignableFrom<BaseEntity>(education);
    }

    [Fact]
    public void Constructor_WithMinimalRequiredArguments_SetsDefaults()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var startDate = new DateOnly(2023, 9, 1);

        // Act
        var education = new Education(
            tenantId,
            "Stanford University",
            startDate,
            endDate: null,
            isCurrent: true);

        // Assert
        Assert.Equal("Stanford University", education.Institution);
        Assert.Equal(startDate, education.StartDate);
        Assert.Null(education.EndDate);
        Assert.True(education.IsCurrent);
        Assert.Null(education.Degree);
        Assert.Null(education.FieldOfStudy);
        Assert.Null(education.Location);
        Assert.Null(education.Description);
        Assert.Equal(0, education.DisplayOrder);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Education(
            emptyTenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true));

        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullInstitution_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Education(
            tenantId,
            null!,
            new DateOnly(2020, 1, 1),
            null,
            true));

        Assert.Equal("institution", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithEmptyOrWhitespaceInstitution_ThrowsArgumentException(string invalidInstitution)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Education(
            tenantId,
            invalidInstitution,
            new DateOnly(2020, 1, 1),
            null,
            true));

        Assert.Equal("institution", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithInstitutionSurroundedByWhitespace_TrimsInstitution()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var education = new Education(
            tenantId,
            "  Harvard University  ",
            new DateOnly(2020, 1, 1),
            null,
            true);

        // Assert
        Assert.Equal("Harvard University", education.Institution);
    }

    [Fact]
    public void Constructor_WithInstitutionAtMaximumLength_AcceptsInstitution()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var institution = new string('U', Education.MaxInstitutionLength);

        // Act
        var education = new Education(
            tenantId,
            institution,
            new DateOnly(2020, 1, 1),
            null,
            true);

        // Assert
        Assert.Equal(Education.MaxInstitutionLength, education.Institution.Length);
        Assert.Equal(institution, education.Institution);
    }

    [Fact]
    public void Constructor_WithInstitutionExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var longInstitution = new string('U', Education.MaxInstitutionLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Education(
            tenantId,
            longInstitution,
            new DateOnly(2020, 1, 1),
            null,
            true));

        Assert.Equal("institution", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithInstitutionAtMaximumLengthSurroundedByWhitespace_TrimsAndAcceptsInstitution()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var rawInstitution = new string('U', Education.MaxInstitutionLength);
        var paddedInstitution = $"  {rawInstitution}  ";

        // Act
        var education = new Education(
            tenantId,
            paddedInstitution,
            new DateOnly(2020, 1, 1),
            null,
            true);

        // Assert
        Assert.Equal(rawInstitution, education.Institution);
        Assert.Equal(Education.MaxInstitutionLength, education.Institution.Length);
    }

    [Fact]
    public void Constructor_WithNullDegree_SetsDegreeToNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: null);

        // Assert
        Assert.Null(education.Degree);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Constructor_WithEmptyOrWhitespaceDegree_NormalizesDegreeToNull(string invalidDegree)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: invalidDegree);

        // Assert
        Assert.Null(education.Degree);
    }

    [Fact]
    public void Constructor_WithDegreeSurroundedByWhitespace_TrimsDegree()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: "  Master of Science  ");

        // Assert
        Assert.Equal("Master of Science", education.Degree);
    }

    [Fact]
    public void Constructor_WithDegreeAtMaximumLength_AcceptsDegree()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var degree = new string('D', Education.MaxDegreeLength);

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: degree);

        // Assert
        Assert.Equal(Education.MaxDegreeLength, education.Degree!.Length);
        Assert.Equal(degree, education.Degree);
    }

    [Fact]
    public void Constructor_WithDegreeExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var longDegree = new string('D', Education.MaxDegreeLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: longDegree));

        Assert.Equal("degree", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullFieldOfStudy_SetsFieldOfStudyToNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            fieldOfStudy: null);

        // Assert
        Assert.Null(education.FieldOfStudy);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Constructor_WithEmptyOrWhitespaceFieldOfStudy_NormalizesFieldOfStudyToNull(string invalidFieldOfStudy)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            fieldOfStudy: invalidFieldOfStudy);

        // Assert
        Assert.Null(education.FieldOfStudy);
    }

    [Fact]
    public void Constructor_WithFieldOfStudySurroundedByWhitespace_TrimsFieldOfStudy()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            fieldOfStudy: "  Computer Engineering  ");

        // Assert
        Assert.Equal("Computer Engineering", education.FieldOfStudy);
    }

    [Fact]
    public void Constructor_WithFieldOfStudyAtMaximumLength_AcceptsFieldOfStudy()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var fieldOfStudy = new string('F', Education.MaxFieldOfStudyLength);

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            fieldOfStudy: fieldOfStudy);

        // Assert
        Assert.Equal(Education.MaxFieldOfStudyLength, education.FieldOfStudy!.Length);
        Assert.Equal(fieldOfStudy, education.FieldOfStudy);
    }

    [Fact]
    public void Constructor_WithFieldOfStudyExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var longField = new string('F', Education.MaxFieldOfStudyLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            fieldOfStudy: longField));

        Assert.Equal("fieldOfStudy", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLocation_SetsLocationToNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            location: null);

        // Assert
        Assert.Null(education.Location);
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
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            location: invalidLocation);

        // Assert
        Assert.Null(education.Location);
    }

    [Fact]
    public void Constructor_WithLocationSurroundedByWhitespace_TrimsLocation()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            location: "  Cambridge, MA  ");

        // Assert
        Assert.Equal("Cambridge, MA", education.Location);
    }

    [Fact]
    public void Constructor_WithLocationAtMaximumLength_AcceptsLocation()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var location = new string('L', Education.MaxLocationLength);

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            location: location);

        // Assert
        Assert.Equal(Education.MaxLocationLength, education.Location!.Length);
        Assert.Equal(location, education.Location);
    }

    [Fact]
    public void Constructor_WithLocationExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var longLocation = new string('L', Education.MaxLocationLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
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
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            description: null);

        // Assert
        Assert.Null(education.Description);
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
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            description: invalidDescription);

        // Assert
        Assert.Null(education.Description);
    }

    [Fact]
    public void Constructor_WithDescriptionSurroundedByWhitespace_TrimsDescription()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            description: "  Completed thesis on AI algorithms.  ");

        // Assert
        Assert.Equal("Completed thesis on AI algorithms.", education.Description);
    }

    [Fact]
    public void Constructor_WithDescriptionAtMaximumLength_AcceptsDescription()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var description = new string('D', Education.MaxDescriptionLength);

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            description: description);

        // Assert
        Assert.Equal(Education.MaxDescriptionLength, education.Description!.Length);
        Assert.Equal(description, education.Description);
    }

    [Fact]
    public void Constructor_WithDescriptionExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var longDescription = new string('D', Education.MaxDescriptionLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            description: longDescription));

        Assert.Equal("description", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithDateOnlyMinValueStartDate_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Education(
            tenantId,
            "MIT",
            DateOnly.MinValue,
            null,
            true));

        Assert.Equal("startDate", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEndDateEqualToStartDate_AcceptsDates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var sameDate = new DateOnly(2022, 6, 1);

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            sameDate,
            sameDate,
            isCurrent: false);

        // Assert
        Assert.Equal(sameDate, education.StartDate);
        Assert.Equal(sameDate, education.EndDate);
    }

    [Fact]
    public void Constructor_WithEndDateAfterStartDate_AcceptsDates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var startDate = new DateOnly(2018, 9, 1);
        var endDate = new DateOnly(2022, 6, 1);

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            startDate,
            endDate,
            isCurrent: false);

        // Assert
        Assert.Equal(startDate, education.StartDate);
        Assert.Equal(endDate, education.EndDate);
    }

    [Fact]
    public void Constructor_WithEndDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var startDate = new DateOnly(2022, 6, 1);
        var endDate = new DateOnly(2018, 9, 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Education(
            tenantId,
            "MIT",
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
        var startDate = new DateOnly(2023, 9, 1);

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            startDate,
            endDate: null,
            isCurrent: true);

        // Assert
        Assert.True(education.IsCurrent);
        Assert.Null(education.EndDate);
    }

    [Fact]
    public void Constructor_WithIsCurrentTrueAndNonNullEndDate_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var startDate = new DateOnly(2022, 9, 1);
        var endDate = new DateOnly(2026, 6, 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Education(
            tenantId,
            "MIT",
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
        var startDate = new DateOnly(2020, 9, 1);

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            startDate,
            endDate: null,
            isCurrent: false);

        // Assert
        Assert.False(education.IsCurrent);
        Assert.Null(education.EndDate);
    }

    [Fact]
    public void Constructor_WithFutureStartDate_AcceptsDates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2));

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            futureDate,
            endDate: null,
            isCurrent: true);

        // Assert
        Assert.Equal(futureDate, education.StartDate);
    }

    [Fact]
    public void Constructor_WithNegativeDisplayOrder_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            displayOrder: -1));

        Assert.Equal("displayOrder", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void Constructor_WithZeroOrPositiveDisplayOrder_AcceptsDisplayOrder(int displayOrder)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            displayOrder: displayOrder);

        // Assert
        Assert.Equal(displayOrder, education.DisplayOrder);
    }

    [Fact]
    public void Constructor_WithValidTenantId_PreservesTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true);

        // Assert
        Assert.Equal(tenantId, education.TenantId);
    }

    [Fact]
    public void UpdateDetails_WithAllPropertiesChanged_UpdatesAllPropertiesAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "Old College",
            new DateOnly(2015, 9, 1),
            new DateOnly(2019, 6, 1),
            isCurrent: false,
            degree: "Associate",
            fieldOfStudy: "General Studies",
            location: "Old Town",
            description: "Old description",
            displayOrder: 0);

        var beforeUpdate = DateTime.UtcNow;

        // Act
        education.UpdateDetails(
            "New University",
            new DateOnly(2019, 9, 1),
            null,
            isCurrent: true,
            degree: "PhD",
            fieldOfStudy: "Quantum Computing",
            location: "New City",
            description: "New thesis on quantum computing.",
            displayOrder: 3);

        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.Equal("New University", education.Institution);
        Assert.Equal(new DateOnly(2019, 9, 1), education.StartDate);
        Assert.Null(education.EndDate);
        Assert.True(education.IsCurrent);
        Assert.Equal("PhD", education.Degree);
        Assert.Equal("Quantum Computing", education.FieldOfStudy);
        Assert.Equal("New City", education.Location);
        Assert.Equal("New thesis on quantum computing.", education.Description);
        Assert.Equal(3, education.DisplayOrder);
        Assert.NotNull(education.UpdatedAtUtc);
        Assert.InRange(education.UpdatedAtUtc.Value, beforeUpdate, afterUpdate);
    }

    [Fact]
    public void UpdateDetails_WithInstitutionOnlyChanged_UpdatesInstitutionAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "Old College",
            new DateOnly(2020, 1, 1),
            null,
            true);

        // Act
        education.UpdateDetails(
            "New College",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: null,
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 0);

        // Assert
        Assert.Equal("New College", education.Institution);
        Assert.NotNull(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithDatesOnlyChanged_UpdatesDatesAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2018, 9, 1),
            new DateOnly(2022, 6, 1),
            isCurrent: false);

        var newStartDate = new DateOnly(2019, 1, 1);
        var newEndDate = new DateOnly(2023, 1, 1);

        // Act
        education.UpdateDetails(
            "MIT",
            newStartDate,
            newEndDate,
            isCurrent: false,
            degree: null,
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 0);

        // Assert
        Assert.Equal(newStartDate, education.StartDate);
        Assert.Equal(newEndDate, education.EndDate);
        Assert.NotNull(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithDegreeOnlyChanged_UpdatesDegreeAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: "B.S.");

        // Act
        education.UpdateDetails(
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: "M.S.",
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 0);

        // Assert
        Assert.Equal("M.S.", education.Degree);
        Assert.NotNull(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithFieldOfStudyOnlyChanged_UpdatesFieldOfStudyAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            fieldOfStudy: "Math");

        // Act
        education.UpdateDetails(
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: null,
            fieldOfStudy: "Physics",
            location: null,
            description: null,
            displayOrder: 0);

        // Assert
        Assert.Equal("Physics", education.FieldOfStudy);
        Assert.NotNull(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithLocationOnlyChanged_UpdatesLocationAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            location: "Boston");

        // Act
        education.UpdateDetails(
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: null,
            fieldOfStudy: null,
            location: "Cambridge",
            description: null,
            displayOrder: 0);

        // Assert
        Assert.Equal("Cambridge", education.Location);
        Assert.NotNull(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithDescriptionOnlyChanged_UpdatesDescriptionAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            description: "Old thesis");

        // Act
        education.UpdateDetails(
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: null,
            fieldOfStudy: null,
            location: null,
            description: "New published paper",
            displayOrder: 0);

        // Assert
        Assert.Equal("New published paper", education.Description);
        Assert.NotNull(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithDisplayOrderOnlyChanged_UpdatesDisplayOrderAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            displayOrder: 0);

        // Act
        education.UpdateDetails(
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: null,
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 7);

        // Assert
        Assert.Equal(7, education.DisplayOrder);
        Assert.NotNull(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithInvalidInstitution_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: null,
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 0));

        Assert.Equal("institution", exception.ParamName);
        Assert.Equal("MIT", education.Institution);
        Assert.Null(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithInvalidDegree_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true);

        var longDegree = new string('D', Education.MaxDegreeLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: longDegree,
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 0));

        Assert.Equal("degree", exception.ParamName);
        Assert.Null(education.Degree);
        Assert.Null(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithInvalidFieldOfStudy_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true);

        var longField = new string('F', Education.MaxFieldOfStudyLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: null,
            fieldOfStudy: longField,
            location: null,
            description: null,
            displayOrder: 0));

        Assert.Equal("fieldOfStudy", exception.ParamName);
        Assert.Null(education.FieldOfStudy);
        Assert.Null(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithInvalidLocation_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true);

        var longLocation = new string('L', Education.MaxLocationLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: null,
            fieldOfStudy: null,
            location: longLocation,
            description: null,
            displayOrder: 0));

        Assert.Equal("location", exception.ParamName);
        Assert.Null(education.Location);
        Assert.Null(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithInvalidDescription_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true);

        var longDescription = new string('D', Education.MaxDescriptionLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: null,
            fieldOfStudy: null,
            location: null,
            description: longDescription,
            displayOrder: 0));

        Assert.Equal("description", exception.ParamName);
        Assert.Null(education.Description);
        Assert.Null(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithInvalidDateRange_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalStartDate = new DateOnly(2018, 9, 1);
        var originalEndDate = new DateOnly(2022, 6, 1);
        var education = new Education(
            tenantId,
            "MIT",
            originalStartDate,
            originalEndDate,
            isCurrent: false);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "MIT",
            new DateOnly(2022, 6, 1),
            new DateOnly(2018, 9, 1),
            isCurrent: false,
            degree: null,
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 0));

        Assert.Equal("endDate", exception.ParamName);
        Assert.Equal(originalStartDate, education.StartDate);
        Assert.Equal(originalEndDate, education.EndDate);
        Assert.Null(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithIsCurrentAndNonNullEndDate_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "MIT",
            new DateOnly(2020, 1, 1),
            new DateOnly(2024, 1, 1),
            isCurrent: true,
            degree: null,
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 0));

        Assert.Equal("endDate", exception.ParamName);
        Assert.True(education.IsCurrent);
        Assert.Null(education.EndDate);
        Assert.Null(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithDateOnlyMinValueStartDate_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalStartDate = new DateOnly(2020, 1, 1);
        var education = new Education(
            tenantId,
            "MIT",
            originalStartDate,
            null,
            true);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "New MIT",
            DateOnly.MinValue,
            null,
            true,
            degree: null,
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 0));

        Assert.Equal("startDate", exception.ParamName);
        Assert.Equal("MIT", education.Institution);
        Assert.Equal(originalStartDate, education.StartDate);
        Assert.Null(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithNegativeDisplayOrder_DoesNotMutateEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            displayOrder: 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => education.UpdateDetails(
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: null,
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: -1));

        Assert.Equal("displayOrder", exception.ParamName);
        Assert.Equal(1, education.DisplayOrder);
        Assert.Null(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithIdenticalNormalizedValues_WhenInitiallyNull_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2018, 9, 1),
            new DateOnly(2022, 6, 1),
            isCurrent: false,
            degree: "B.S.",
            fieldOfStudy: "CS",
            location: "Cambridge",
            description: "Honors",
            displayOrder: 2);

        Assert.Null(education.UpdatedAtUtc);

        // Act - reapply identical values
        education.UpdateDetails(
            "MIT",
            new DateOnly(2018, 9, 1),
            new DateOnly(2022, 6, 1),
            isCurrent: false,
            degree: "B.S.",
            fieldOfStudy: "CS",
            location: "Cambridge",
            description: "Honors",
            displayOrder: 2);

        // Assert
        Assert.Null(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithIdenticalNormalizedValues_WhenAlreadyUpdated_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true);

        education.UpdateDetails(
            "Stanford",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: "M.S.",
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 1);

        var originalUpdatedAt = education.UpdatedAtUtc;
        Assert.NotNull(originalUpdatedAt);

        // Act - reapply identical values
        education.UpdateDetails(
            "Stanford",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: "M.S.",
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 1);

        // Assert
        Assert.Equal(originalUpdatedAt, education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithDifferentStringInstancesHavingSameNormalizedValues_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true);

        education.UpdateDetails(
            "Stanford University",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: "Bachelor of Science",
            fieldOfStudy: "Computer Science",
            location: "Stanford, CA",
            description: "Magna cum laude",
            displayOrder: 1);

        var originalUpdatedAt = education.UpdatedAtUtc;
        Assert.NotNull(originalUpdatedAt);

        // Act - reapply equivalent normalized values with extra padding
        education.UpdateDetails(
            "  Stanford University  ",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: "  Bachelor of Science  ",
            fieldOfStudy: "  Computer Science  ",
            location: "  Stanford, CA  ",
            description: "  Magna cum laude  ",
            displayOrder: 1);

        // Assert
        Assert.Equal(originalUpdatedAt, education.UpdatedAtUtc);
        Assert.Equal("Stanford University", education.Institution);
        Assert.Equal("Bachelor of Science", education.Degree);
        Assert.Equal("Computer Science", education.FieldOfStudy);
        Assert.Equal("Stanford, CA", education.Location);
        Assert.Equal("Magna cum laude", education.Description);
    }

    [Fact]
    public void UpdateDetails_WithActualChanges_UpdatesUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true);

        Assert.Null(education.UpdatedAtUtc);
        var before = DateTime.UtcNow;

        // Act
        education.UpdateDetails(
            "Harvard",
            new DateOnly(2020, 1, 1),
            null,
            true,
            degree: null,
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 0);

        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(education.UpdatedAtUtc);
        Assert.InRange(education.UpdatedAtUtc.Value, before, after);
    }

    [Fact]
    public void UpdateDetails_WhenCalled_DoesNotModifyId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true);

        var originalId = education.Id;

        // Act
        education.UpdateDetails(
            "Harvard",
            new DateOnly(2021, 1, 1),
            null,
            true,
            degree: null,
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 1);

        // Assert
        Assert.Equal(originalId, education.Id);
    }

    [Fact]
    public void UpdateDetails_WhenCalled_DoesNotModifyTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2020, 1, 1),
            null,
            true);

        var originalTenantId = education.TenantId;

        // Act
        education.UpdateDetails(
            "Harvard",
            new DateOnly(2021, 1, 1),
            null,
            true,
            degree: null,
            fieldOfStudy: null,
            location: null,
            description: null,
            displayOrder: 1);

        // Assert
        Assert.Equal(originalTenantId, education.TenantId);
    }

    [Fact]
    public void UpdateDetails_WithNullOrWhitespaceOptionalStrings_ClearsOptionalFields()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "MIT",
            new DateOnly(2018, 9, 1),
            new DateOnly(2022, 6, 1),
            isCurrent: false,
            degree: "B.S.",
            fieldOfStudy: "Computer Science",
            location: "Cambridge, MA",
            description: "Completed with honors.");

        Assert.Equal("B.S.", education.Degree);
        Assert.Equal("Computer Science", education.FieldOfStudy);
        Assert.Equal("Cambridge, MA", education.Location);
        Assert.Equal("Completed with honors.", education.Description);

        // Act - clear all optional strings with whitespace or null
        education.UpdateDetails(
            "MIT",
            new DateOnly(2018, 9, 1),
            new DateOnly(2022, 6, 1),
            isCurrent: false,
            degree: "   ",
            fieldOfStudy: "",
            location: null,
            description: "\t",
            displayOrder: 0);

        // Assert
        Assert.Null(education.Degree);
        Assert.Null(education.FieldOfStudy);
        Assert.Null(education.Location);
        Assert.Null(education.Description);
        Assert.NotNull(education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenEarlyFieldsChangedButDegreeInvalid_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalStartDate = new DateOnly(2018, 9, 1);
        var originalEndDate = new DateOnly(2022, 6, 1);
        var education = new Education(
            tenantId,
            "Original University",
            originalStartDate,
            originalEndDate,
            isCurrent: false,
            degree: "B.S.",
            fieldOfStudy: "Physics",
            location: "Boston",
            description: "Original description",
            displayOrder: 1);

        var originalId = education.Id;
        var originalTenantId = education.TenantId;
        var originalUpdatedAt = education.UpdatedAtUtc;
        var invalidDegree = new string('D', Education.MaxDegreeLength + 1);

        // Act & Assert - Institution, StartDate, EndDate changed to new valid values, but Degree exceeds max length
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "New University",
            new DateOnly(2019, 9, 1),
            new DateOnly(2023, 6, 1),
            isCurrent: false,
            degree: invalidDegree,
            fieldOfStudy: "Astronomy",
            location: "New York",
            description: "New description",
            displayOrder: 5));

        Assert.Equal("degree", exception.ParamName);
        Assert.Equal("Original University", education.Institution);
        Assert.Equal(originalStartDate, education.StartDate);
        Assert.Equal(originalEndDate, education.EndDate);
        Assert.False(education.IsCurrent);
        Assert.Equal("B.S.", education.Degree);
        Assert.Equal("Physics", education.FieldOfStudy);
        Assert.Equal("Boston", education.Location);
        Assert.Equal("Original description", education.Description);
        Assert.Equal(1, education.DisplayOrder);
        Assert.Equal(originalId, education.Id);
        Assert.Equal(originalTenantId, education.TenantId);
        Assert.Equal(originalUpdatedAt, education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenEarlyFieldsChangedButFieldOfStudyInvalid_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalStartDate = new DateOnly(2018, 9, 1);
        var education = new Education(
            tenantId,
            "Original University",
            originalStartDate,
            null,
            true,
            degree: "B.S.",
            fieldOfStudy: "Physics");

        var originalUpdatedAt = education.UpdatedAtUtc;
        var invalidFieldOfStudy = new string('F', Education.MaxFieldOfStudyLength + 1);

        // Act & Assert - Institution and Degree changed, but FieldOfStudy exceeds max length
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "New University",
            new DateOnly(2019, 9, 1),
            null,
            true,
            degree: "M.S.",
            fieldOfStudy: invalidFieldOfStudy,
            location: "New York",
            description: "New description",
            displayOrder: 5));

        Assert.Equal("fieldOfStudy", exception.ParamName);
        Assert.Equal("Original University", education.Institution);
        Assert.Equal(originalStartDate, education.StartDate);
        Assert.Equal("B.S.", education.Degree);
        Assert.Equal("Physics", education.FieldOfStudy);
        Assert.Equal(originalUpdatedAt, education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenEarlyFieldsChangedButLocationInvalid_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "Original University",
            new DateOnly(2018, 9, 1),
            null,
            true,
            degree: "B.S.",
            location: "Boston");

        var originalUpdatedAt = education.UpdatedAtUtc;
        var invalidLocation = new string('L', Education.MaxLocationLength + 1);

        // Act & Assert - Institution and Degree changed, but Location exceeds max length
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "New University",
            new DateOnly(2019, 9, 1),
            null,
            true,
            degree: "M.S.",
            fieldOfStudy: "Math",
            location: invalidLocation,
            description: null,
            displayOrder: 0));

        Assert.Equal("location", exception.ParamName);
        Assert.Equal("Original University", education.Institution);
        Assert.Equal("B.S.", education.Degree);
        Assert.Equal("Boston", education.Location);
        Assert.Equal(originalUpdatedAt, education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenEarlyFieldsChangedButDescriptionInvalid_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "Original University",
            new DateOnly(2018, 9, 1),
            null,
            true);

        var originalUpdatedAt = education.UpdatedAtUtc;
        var invalidDescription = new string('D', Education.MaxDescriptionLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "New University",
            new DateOnly(2019, 9, 1),
            null,
            true,
            degree: "M.S.",
            fieldOfStudy: "Math",
            location: "New York",
            description: invalidDescription,
            displayOrder: 0));

        Assert.Equal("description", exception.ParamName);
        Assert.Equal("Original University", education.Institution);
        Assert.Null(education.Description);
        Assert.Equal(originalUpdatedAt, education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenEarlyFieldsChangedButDatesInvalid_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalStartDate = new DateOnly(2018, 9, 1);
        var originalEndDate = new DateOnly(2022, 6, 1);
        var education = new Education(
            tenantId,
            "Original University",
            originalStartDate,
            originalEndDate,
            isCurrent: false);

        var originalUpdatedAt = education.UpdatedAtUtc;

        // Act & Assert - pass changed institution, but invalid EndDate < StartDate
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "Changed University",
            new DateOnly(2023, 6, 1),
            new DateOnly(2020, 1, 1),
            isCurrent: false,
            degree: "B.S.",
            fieldOfStudy: "CS",
            location: null,
            description: null,
            displayOrder: 0));

        Assert.Equal("endDate", exception.ParamName);
        Assert.Equal("Original University", education.Institution);
        Assert.Equal(originalStartDate, education.StartDate);
        Assert.Equal(originalEndDate, education.EndDate);
        Assert.Equal(originalUpdatedAt, education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenEarlyFieldsChangedButIsCurrentWithEndDate_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "Original University",
            new DateOnly(2018, 9, 1),
            new DateOnly(2022, 6, 1),
            isCurrent: false);

        var originalUpdatedAt = education.UpdatedAtUtc;

        // Act & Assert - pass changed institution, but isCurrent: true with EndDate
        var exception = Assert.Throws<ArgumentException>(() => education.UpdateDetails(
            "Changed University",
            new DateOnly(2023, 6, 1),
            new DateOnly(2025, 1, 1),
            isCurrent: true,
            degree: "B.S.",
            fieldOfStudy: "CS",
            location: null,
            description: null,
            displayOrder: 0));

        Assert.Equal("endDate", exception.ParamName);
        Assert.Equal("Original University", education.Institution);
        Assert.False(education.IsCurrent);
        Assert.Equal(new DateOnly(2022, 6, 1), education.EndDate);
        Assert.Equal(originalUpdatedAt, education.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenEarlyFieldsChangedButDisplayOrderNegative_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var education = new Education(
            tenantId,
            "Original University",
            new DateOnly(2018, 9, 1),
            null,
            true,
            displayOrder: 1);

        var originalUpdatedAt = education.UpdatedAtUtc;

        // Act & Assert - pass changed institution, degree, but displayOrder = -1
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => education.UpdateDetails(
            "Changed University",
            new DateOnly(2019, 1, 1),
            null,
            true,
            degree: "M.S.",
            fieldOfStudy: "Math",
            location: "Chicago",
            description: "Thesis",
            displayOrder: -1));

        Assert.Equal("displayOrder", exception.ParamName);
        Assert.Equal("Original University", education.Institution);
        Assert.Equal(1, education.DisplayOrder);
        Assert.Equal(originalUpdatedAt, education.UpdatedAtUtc);
    }
}
