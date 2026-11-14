using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;

namespace Profolio.UnitTests;

public class DeveloperProfileTests
{
    [Fact]
    public void Constructor_WithValidArguments_CreatesDeveloperProfile()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var fullName = "Ada Lovelace";
        var headline = "First Computer Programmer";

        // Act
        var profile = new DeveloperProfile(tenantId, fullName, headline);

        // Assert
        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal(tenantId, profile.TenantId);
        Assert.Equal(fullName, profile.FullName);
        Assert.Equal(headline, profile.Headline);
        Assert.Null(profile.Bio);
        Assert.Null(profile.Location);
        Assert.Null(profile.PublicContactEmail);
        Assert.True(profile.CreatedAtUtc <= DateTime.UtcNow);
        Assert.Null(profile.UpdatedAtUtc);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;
        var fullName = "Ada Lovelace";
        var headline = "First Computer Programmer";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DeveloperProfile(emptyTenantId, fullName, headline));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrWhitespaceFullName_ThrowsArgumentException(string? invalidFullName)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var headline = "First Computer Programmer";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DeveloperProfile(tenantId, invalidFullName!, headline));
        Assert.Equal("fullName", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrWhitespaceHeadline_ThrowsArgumentException(string? invalidHeadline)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var fullName = "Ada Lovelace";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DeveloperProfile(tenantId, fullName, invalidHeadline!));
        Assert.Equal("headline", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithUntrimmedStrings_TrimsFullNameAndHeadline()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var untrimmedFullName = "   Ada Lovelace   ";
        var untrimmedHeadline = "   First Computer Programmer   ";

        // Act
        var profile = new DeveloperProfile(tenantId, untrimmedFullName, untrimmedHeadline);

        // Assert
        Assert.Equal("Ada Lovelace", profile.FullName);
        Assert.Equal("First Computer Programmer", profile.Headline);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateBasicInformation_WithEmptyOrWhitespaceBioAndLocation_NormalizesToNull(string? emptyValue)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");

        // Act
        profile.UpdateBasicInformation("Ada Lovelace", "Programmer", emptyValue, emptyValue);

        // Assert
        Assert.Null(profile.Bio);
        Assert.Null(profile.Location);
    }

    [Fact]
    public void UpdateBasicInformation_PreservesTenantId()
    {
        // Arrange
        var originalTenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(originalTenantId, "Ada Lovelace", "Programmer");

        // Act
        profile.UpdateBasicInformation("Ada King", "Countess of Lovelace", "Mathematician", "London");

        // Assert
        Assert.Equal(originalTenantId, profile.TenantId);
    }

    [Fact]
    public void UpdateBasicInformation_PreservesId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var originalId = profile.Id;

        // Act
        profile.UpdateBasicInformation("Ada King", "Countess of Lovelace", "Mathematician", "London");

        // Assert
        Assert.Equal(originalId, profile.Id);
    }

    [Fact]
    public void UpdateBasicInformation_PopulatesUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        Assert.Null(profile.UpdatedAtUtc);

        // Act
        profile.UpdateBasicInformation("Ada King", "Countess of Lovelace", "Mathematician", "London");

        // Assert
        Assert.NotNull(profile.UpdatedAtUtc);
        Assert.True(profile.UpdatedAtUtc >= profile.CreatedAtUtc);
        Assert.True(profile.UpdatedAtUtc <= DateTime.UtcNow);
    }

    [Fact]
    public void SetPublicContactEmail_WithValidEmail_SetsPropertyAndUpdatesTimestamp()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var email = new EmailAddress("ada.lovelace@example.com");

        // Act
        profile.SetPublicContactEmail(email);

        // Assert
        Assert.NotNull(profile.PublicContactEmail);
        Assert.Equal(email, profile.PublicContactEmail);
        Assert.NotNull(profile.UpdatedAtUtc);
        Assert.True(profile.UpdatedAtUtc >= profile.CreatedAtUtc);
    }

    [Fact]
    public void SetPublicContactEmail_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => profile.SetPublicContactEmail(null!));
        Assert.Equal("contactEmail", exception.ParamName);
    }

    [Fact]
    public void SetPublicContactEmail_PreservesIdAndTenantId()
    {
        // Arrange
        var originalTenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(originalTenantId, "Ada Lovelace", "Programmer");
        var originalId = profile.Id;
        var email = new EmailAddress("ada.lovelace@example.com");

        // Act
        profile.SetPublicContactEmail(email);

        // Assert
        Assert.Equal(originalId, profile.Id);
        Assert.Equal(originalTenantId, profile.TenantId);
    }

    [Fact]
    public void SetPublicContactEmail_WithSameEmailInstance_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var email = new EmailAddress("ada.lovelace@example.com");
        profile.SetPublicContactEmail(email);
        var originalUpdatedAt = profile.UpdatedAtUtc;

        // Act
        profile.SetPublicContactEmail(email);

        // Assert
        Assert.Equal(originalUpdatedAt, profile.UpdatedAtUtc);
    }

    [Fact]
    public void SetPublicContactEmail_WithDifferentInstanceHavingSameNormalizedValue_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var email1 = new EmailAddress("ada.lovelace@example.com");
        var email2 = new EmailAddress("ada.lovelace@EXAMPLE.com");
        profile.SetPublicContactEmail(email1);
        var originalUpdatedAt = profile.UpdatedAtUtc;

        // Act
        profile.SetPublicContactEmail(email2);

        // Assert
        Assert.Equal(originalUpdatedAt, profile.UpdatedAtUtc);
    }

    [Fact]
    public void ClearPublicContactEmail_WhenEmailExists_ClearsEmailAndUpdatesTimestamp()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        profile.SetPublicContactEmail(new EmailAddress("ada.lovelace@example.com"));
        Assert.NotNull(profile.PublicContactEmail);

        // Act
        profile.ClearPublicContactEmail();

        // Assert
        Assert.Null(profile.PublicContactEmail);
        Assert.NotNull(profile.UpdatedAtUtc);
    }

    [Fact]
    public void ClearPublicContactEmail_WhenAlreadyNull_DoesNotModifyEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        Assert.Null(profile.PublicContactEmail);
        Assert.Null(profile.UpdatedAtUtc);

        // Act
        profile.ClearPublicContactEmail();

        // Assert
        Assert.Null(profile.PublicContactEmail);
        Assert.Null(profile.UpdatedAtUtc);
    }
}
