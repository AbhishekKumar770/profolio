using Profolio.Domain.Entities;

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
}
