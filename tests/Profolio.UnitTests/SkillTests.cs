using Profolio.Domain.Common;
using Profolio.Domain.Entities;

namespace Profolio.UnitTests;

public class SkillTests
{
    [Fact]
    public void Constructor_WithValidArguments_CreatesSkill()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var name = "C#";
        var category = "Backend";
        var displayOrder = 1;

        // Act
        var skill = new Skill(tenantId, name, category, displayOrder);

        // Assert
        Assert.NotEqual(Guid.Empty, skill.Id);
        Assert.Equal(tenantId, skill.TenantId);
        Assert.Equal("C#", skill.Name);
        Assert.Equal("Backend", skill.Category);
        Assert.Equal(1, skill.DisplayOrder);
        Assert.True(skill.CreatedAtUtc <= DateTime.UtcNow);
        Assert.Null(skill.UpdatedAtUtc);
        Assert.IsAssignableFrom<ITenantEntity>(skill);
        Assert.IsAssignableFrom<BaseEntity>(skill);
    }

    [Fact]
    public void Constructor_WithDefaultCategoryAndDisplayOrder_UsesDefaults()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var name = "Git";

        // Act
        var skill = new Skill(tenantId, name);

        // Assert
        Assert.Equal("Git", skill.Name);
        Assert.Null(skill.Category);
        Assert.Equal(0, skill.DisplayOrder);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Skill(emptyTenantId, "C#", "Backend", 0));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Skill(tenantId, null!, "Backend", 0));
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Skill(tenantId, string.Empty, "Backend", 0));
        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Constructor_WithWhitespaceName_ThrowsArgumentException(string whitespaceName)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Skill(tenantId, whitespaceName, "Backend", 0));
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNameSurroundedByWhitespace_TrimsName()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var rawName = "  PostgreSQL  ";

        // Act
        var skill = new Skill(tenantId, rawName, "Database", 0);

        // Assert
        Assert.Equal("PostgreSQL", skill.Name);
    }

    [Fact]
    public void Constructor_WithNameAtMaximumLength_AcceptsName()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var nameAtMax = new string('x', Skill.MaxNameLength);

        // Act
        var skill = new Skill(tenantId, nameAtMax, "Category", 0);

        // Assert
        Assert.Equal(Skill.MaxNameLength, skill.Name.Length);
        Assert.Equal(nameAtMax, skill.Name);
    }

    [Fact]
    public void Constructor_WithNameExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var nameExceedingMax = new string('x', Skill.MaxNameLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Skill(tenantId, nameExceedingMax, "Category", 0));
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNameAtMaxAfterTrimming_AcceptsName()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var untrimmedName = $"  {new string('x', Skill.MaxNameLength)}  ";

        // Act
        var skill = new Skill(tenantId, untrimmedName, "Category", 0);

        // Assert
        Assert.Equal(Skill.MaxNameLength, skill.Name.Length);
    }

    [Fact]
    public void Constructor_WithNullCategory_SetsCategoryToNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var skill = new Skill(tenantId, "Docker", null, 0);

        // Assert
        Assert.Null(skill.Category);
    }

    [Fact]
    public void Constructor_WithEmptyCategory_NormalizesCategoryToNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var skill = new Skill(tenantId, "Docker", string.Empty, 0);

        // Assert
        Assert.Null(skill.Category);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithWhitespaceCategory_NormalizesCategoryToNull(string whitespaceCategory)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var skill = new Skill(tenantId, "Docker", whitespaceCategory, 0);

        // Assert
        Assert.Null(skill.Category);
    }

    [Fact]
    public void Constructor_WithCategorySurroundedByWhitespace_TrimsCategory()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var rawCategory = "  DevOps & Cloud  ";

        // Act
        var skill = new Skill(tenantId, "Kubernetes", rawCategory, 0);

        // Assert
        Assert.Equal("DevOps & Cloud", skill.Category);
    }

    [Fact]
    public void Constructor_WithCategoryAtMaximumLength_AcceptsCategory()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var categoryAtMax = new string('c', Skill.MaxCategoryLength);

        // Act
        var skill = new Skill(tenantId, "Kubernetes", categoryAtMax, 0);

        // Assert
        Assert.Equal(Skill.MaxCategoryLength, skill.Category!.Length);
        Assert.Equal(categoryAtMax, skill.Category);
    }

    [Fact]
    public void Constructor_WithCategoryExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var categoryExceedingMax = new string('c', Skill.MaxCategoryLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Skill(tenantId, "Kubernetes", categoryExceedingMax, 0));
        Assert.Equal("category", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithCategoryAtMaxAfterTrimming_AcceptsCategory()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var untrimmedCategory = $"  {new string('c', Skill.MaxCategoryLength)}  ";

        // Act
        var skill = new Skill(tenantId, "Kubernetes", untrimmedCategory, 0);

        // Assert
        Assert.Equal(Skill.MaxCategoryLength, skill.Category!.Length);
    }

    [Fact]
    public void Constructor_WithNegativeDisplayOrder_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var negativeDisplayOrder = -1;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Skill(tenantId, "C#", "Backend", negativeDisplayOrder));
        Assert.Equal("displayOrder", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithZeroDisplayOrder_AcceptsDisplayOrder()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var skill = new Skill(tenantId, "C#", "Backend", 0);

        // Assert
        Assert.Equal(0, skill.DisplayOrder);
    }

    [Fact]
    public void Constructor_WithPositiveDisplayOrder_AcceptsDisplayOrder()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var skill = new Skill(tenantId, "C#", "Backend", 42);

        // Assert
        Assert.Equal(42, skill.DisplayOrder);
    }

    [Fact]
    public void Constructor_WithValidTenantId_PreservesTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var skill = new Skill(tenantId, "C#", "Backend", 1);

        // Assert
        Assert.Equal(tenantId, skill.TenantId);
    }

    [Fact]
    public void UpdateDetails_WithAllPropertiesChanged_UpdatesAllPropertiesAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);
        Assert.Null(skill.UpdatedAtUtc);

        var beforeUpdate = DateTime.UtcNow;

        // Act
        skill.UpdateDetails("TypeScript", "Frontend", 5);

        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.Equal("TypeScript", skill.Name);
        Assert.Equal("Frontend", skill.Category);
        Assert.Equal(5, skill.DisplayOrder);
        Assert.NotNull(skill.UpdatedAtUtc);
        Assert.InRange(skill.UpdatedAtUtc.Value, beforeUpdate, afterUpdate);
    }

    [Fact]
    public void UpdateDetails_WithNameOnlyChanged_UpdatesNameAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);

        // Act
        skill.UpdateDetails("F#", "Backend", 0);

        // Assert
        Assert.Equal("F#", skill.Name);
        Assert.Equal("Backend", skill.Category);
        Assert.Equal(0, skill.DisplayOrder);
        Assert.NotNull(skill.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithCategoryOnlyChanged_UpdatesCategoryAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);

        // Act
        skill.UpdateDetails("C#", "Languages", 0);

        // Assert
        Assert.Equal("C#", skill.Name);
        Assert.Equal("Languages", skill.Category);
        Assert.Equal(0, skill.DisplayOrder);
        Assert.NotNull(skill.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithDisplayOrderOnlyChanged_UpdatesDisplayOrderAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);

        // Act
        skill.UpdateDetails("C#", "Backend", 10);

        // Assert
        Assert.Equal("C#", skill.Name);
        Assert.Equal("Backend", skill.Category);
        Assert.Equal(10, skill.DisplayOrder);
        Assert.NotNull(skill.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithNullName_ThrowsArgumentNullExceptionAndDoesNotMutateState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => skill.UpdateDetails(null!, "Backend", 0));
        Assert.Equal("name", exception.ParamName);
        Assert.Equal("C#", skill.Name);
        Assert.Equal("Backend", skill.Category);
        Assert.Equal(0, skill.DisplayOrder);
        Assert.Null(skill.UpdatedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void UpdateDetails_WithEmptyOrWhitespaceName_ThrowsArgumentExceptionAndDoesNotMutateState(string invalidName)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => skill.UpdateDetails(invalidName, "Backend", 0));
        Assert.Equal("name", exception.ParamName);
        Assert.Equal("C#", skill.Name);
        Assert.Equal("Backend", skill.Category);
        Assert.Equal(0, skill.DisplayOrder);
        Assert.Null(skill.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithNameExceedingMaximumLength_ThrowsArgumentExceptionAndDoesNotMutateState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);
        var longName = new string('x', Skill.MaxNameLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => skill.UpdateDetails(longName, "Backend", 0));
        Assert.Equal("name", exception.ParamName);
        Assert.Equal("C#", skill.Name);
        Assert.Equal("Backend", skill.Category);
        Assert.Equal(0, skill.DisplayOrder);
        Assert.Null(skill.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithCategoryExceedingMaximumLength_ThrowsArgumentExceptionAndDoesNotMutateState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);
        var longCategory = new string('y', Skill.MaxCategoryLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => skill.UpdateDetails("C#", longCategory, 0));
        Assert.Equal("category", exception.ParamName);
        Assert.Equal("C#", skill.Name);
        Assert.Equal("Backend", skill.Category);
        Assert.Equal(0, skill.DisplayOrder);
        Assert.Null(skill.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithNegativeDisplayOrder_ThrowsArgumentOutOfRangeExceptionAndDoesNotMutateState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => skill.UpdateDetails("C#", "Backend", -1));
        Assert.Equal("displayOrder", exception.ParamName);
        Assert.Equal("C#", skill.Name);
        Assert.Equal("Backend", skill.Category);
        Assert.Equal(0, skill.DisplayOrder);
        Assert.Null(skill.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithIdenticalNormalizedValues_WhenInitiallyNull_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 1);
        Assert.Null(skill.UpdatedAtUtc);

        // Act - reapply identical values
        skill.UpdateDetails("C#", "Backend", 1);

        // Assert
        Assert.Null(skill.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithIdenticalNormalizedValues_WhenAlreadyUpdated_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);
        skill.UpdateDetails("TypeScript", "Frontend", 2);
        var originalUpdatedAt = skill.UpdatedAtUtc;
        Assert.NotNull(originalUpdatedAt);

        // Act - reapply identical values
        skill.UpdateDetails("TypeScript", "Frontend", 2);

        // Assert
        Assert.Equal(originalUpdatedAt, skill.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithDifferentStringInstancesHavingSameNormalizedValues_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);
        skill.UpdateDetails("TypeScript", "Frontend", 2);
        var originalUpdatedAt = skill.UpdatedAtUtc;
        Assert.NotNull(originalUpdatedAt);

        // Act - reapply equivalent normalized values via different string instances and extra padding
        var equivalentName = "  TypeScript  ";
        var equivalentCategory = "  Frontend  ";
        skill.UpdateDetails(equivalentName, equivalentCategory, 2);

        // Assert
        Assert.Equal(originalUpdatedAt, skill.UpdatedAtUtc);
        Assert.Equal("TypeScript", skill.Name);
        Assert.Equal("Frontend", skill.Category);
    }

    [Fact]
    public void UpdateDetails_WithActualChanges_UpdatesUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);
        Assert.Null(skill.UpdatedAtUtc);

        var before = DateTime.UtcNow;

        // Act
        skill.UpdateDetails("Go", "Backend", 0);

        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(skill.UpdatedAtUtc);
        Assert.InRange(skill.UpdatedAtUtc.Value, before, after);
    }

    [Fact]
    public void UpdateDetails_WhenCalled_DoesNotModifyId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);
        var originalId = skill.Id;

        // Act
        skill.UpdateDetails("Rust", "Systems", 3);

        // Assert
        Assert.Equal(originalId, skill.Id);
    }

    [Fact]
    public void UpdateDetails_WhenCalled_DoesNotModifyTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);
        var originalTenantId = skill.TenantId;

        // Act
        skill.UpdateDetails("Rust", "Systems", 3);

        // Assert
        Assert.Equal(originalTenantId, skill.TenantId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void UpdateDetails_WithEmptyOrWhitespaceCategory_NormalizesCategoryToNull(string emptyOrWhitespaceCategory)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);

        // Act
        skill.UpdateDetails("C#", emptyOrWhitespaceCategory, 0);

        // Assert
        Assert.Null(skill.Category);
        Assert.NotNull(skill.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithNullCategory_ClearsCategory()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 0);
        Assert.Equal("Backend", skill.Category);

        // Act
        skill.UpdateDetails("C#", null, 0);

        // Assert
        Assert.Null(skill.Category);
        Assert.NotNull(skill.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenNameValidButCategoryInvalid_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 1);
        var originalId = skill.Id;
        var originalTenantId = skill.TenantId;
        var originalUpdatedAt = skill.UpdatedAtUtc;
        var longCategory = new string('y', Skill.MaxCategoryLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => skill.UpdateDetails("ASP.NET Core", longCategory, 2));
        Assert.Equal("category", exception.ParamName);
        Assert.Equal("C#", skill.Name);
        Assert.Equal("Backend", skill.Category);
        Assert.Equal(1, skill.DisplayOrder);
        Assert.Equal(originalId, skill.Id);
        Assert.Equal(originalTenantId, skill.TenantId);
        Assert.Equal(originalUpdatedAt, skill.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenNameAndCategoryValidButDisplayOrderNegative_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "C#", "Backend", 1);
        var originalId = skill.Id;
        var originalTenantId = skill.TenantId;
        var originalUpdatedAt = skill.UpdatedAtUtc;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => skill.UpdateDetails("ASP.NET Core", "Web", -1));
        Assert.Equal("displayOrder", exception.ParamName);
        Assert.Equal("C#", skill.Name);
        Assert.Equal("Backend", skill.Category);
        Assert.Equal(1, skill.DisplayOrder);
        Assert.Equal(originalId, skill.Id);
        Assert.Equal(originalTenantId, skill.TenantId);
        Assert.Equal(originalUpdatedAt, skill.UpdatedAtUtc);
    }
}
