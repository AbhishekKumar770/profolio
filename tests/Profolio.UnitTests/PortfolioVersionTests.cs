using System.Reflection;
using Profolio.Domain.Entities;

namespace Profolio.UnitTests;

public class PortfolioVersionTests
{
    private const string SampleSnapshotJson = "{\"profile\":{\"name\":\"Ada Lovelace\"},\"projects\":[]}";

    [Fact]
    public void Constructor_WithValidArgumentsAndDefaultSchemaVersion_InitializesProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var versionNumber = 1;

        // Act
        var version = new PortfolioVersion(tenantId, versionNumber, SampleSnapshotJson);

        // Assert
        Assert.NotEqual(Guid.Empty, version.Id);
        Assert.Equal(tenantId, version.TenantId);
        Assert.Equal(versionNumber, version.VersionNumber);
        Assert.Equal(SampleSnapshotJson, version.SnapshotJson);
        Assert.Equal(1, version.SnapshotSchemaVersion);
        Assert.True(version.CreatedAtUtc <= DateTime.UtcNow);
        Assert.Null(version.UpdatedAtUtc);
    }

    [Fact]
    public void Constructor_WithExplicitSchemaVersion_SetsSchemaVersion()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var versionNumber = 5;
        var schemaVersion = 2;

        // Act
        var version = new PortfolioVersion(tenantId, versionNumber, SampleSnapshotJson, schemaVersion);

        // Assert
        Assert.Equal(schemaVersion, version.SnapshotSchemaVersion);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new PortfolioVersion(Guid.Empty, 1, SampleSnapshotJson));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_WithZeroOrNegativeVersionNumber_ThrowsArgumentOutOfRangeException(int invalidVersion)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PortfolioVersion(Guid.NewGuid(), invalidVersion, SampleSnapshotJson));
        Assert.Equal("versionNumber", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_WithZeroOrNegativeSchemaVersion_ThrowsArgumentOutOfRangeException(int invalidSchema)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PortfolioVersion(Guid.NewGuid(), 1, SampleSnapshotJson, invalidSchema));
        Assert.Equal("snapshotSchemaVersion", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullSnapshotJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new PortfolioVersion(Guid.NewGuid(), 1, null!));
        Assert.Equal("snapshotJson", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void Constructor_WithEmptyOrWhitespaceSnapshotJson_ThrowsArgumentException(string invalidJson)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new PortfolioVersion(Guid.NewGuid(), 1, invalidJson));
        Assert.Equal("snapshotJson", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithSnapshotJsonAtMaximumLength_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var maxJson = new string('j', PortfolioVersion.MaxSnapshotJsonLength);

        // Act
        var version = new PortfolioVersion(tenantId, 1, maxJson);

        // Assert
        Assert.Equal(PortfolioVersion.MaxSnapshotJsonLength, version.SnapshotJson.Length);
    }

    [Fact]
    public void Constructor_WithSnapshotJsonExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var excessiveJson = new string('j', PortfolioVersion.MaxSnapshotJsonLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new PortfolioVersion(Guid.NewGuid(), 1, excessiveJson));
        Assert.Equal("snapshotJson", exception.ParamName);
        Assert.Contains("maximum allowed length", exception.Message);
        Assert.Contains("characters", exception.Message);
    }

    [Fact]
    public void UpdatedAtUtc_RemainsNullThroughoutLifecycle()
    {
        // Arrange & Act
        var version = new PortfolioVersion(Guid.NewGuid(), 1, SampleSnapshotJson);

        // Assert
        Assert.Null(version.UpdatedAtUtc);
    }

    [Fact]
    public void Immutability_HasNoPublicMutatorsAndNoPublicPropertySetters()
    {
        // Arrange
        var properties = typeof(PortfolioVersion).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var methods = typeof(PortfolioVersion).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        // Assert - all property setters must be non-public
        foreach (var property in properties)
        {
            Assert.False(property.SetMethod?.IsPublic ?? false, $"Property {property.Name} has a public setter.");
        }

        // Assert - declared public methods should only be property getters or standard object overrides
        var nonGetterMethods = methods.Where(m => !m.IsSpecialName).ToList();
        Assert.Empty(nonGetterMethods);
    }
}
