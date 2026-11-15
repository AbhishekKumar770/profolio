using Profolio.Domain.Common;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;

namespace Profolio.UnitTests;

public class TenantTests
{
    [Fact]
    public void Constructor_WithValidArguments_CreatesTenant()
    {
        // Arrange
        var ownerUserId = "aspnetcore-identity-user-123";
        var username = new Username("adalovelace");

        // Act
        var tenant = new Tenant(ownerUserId, username);

        // Assert
        Assert.NotEqual(Guid.Empty, tenant.Id);
        Assert.Equal(ownerUserId, tenant.OwnerUserId);
        Assert.Equal(username, tenant.Username);
        Assert.Null(tenant.UpdatedAtUtc);
        Assert.True(tenant.CreatedAtUtc <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithNullOwnerUserId_ThrowsArgumentNullException()
    {
        // Arrange
        var username = new Username("adalovelace");

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Tenant(null!, username));
        Assert.Equal("ownerUserId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Constructor_WithEmptyOrWhitespaceOwnerUserId_ThrowsArgumentException(string invalidOwnerUserId)
    {
        // Arrange
        var username = new Username("adalovelace");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Tenant(invalidOwnerUserId, username));
        Assert.Equal("ownerUserId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullUsername_ThrowsArgumentNullException()
    {
        // Arrange
        var ownerUserId = "user-123";

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Tenant(ownerUserId, null!));
        Assert.Equal("username", exception.ParamName);
    }

    [Fact]
    public void Constructor_PreservesOwnerUserIdExactlyWithoutNormalization()
    {
        // Arrange - case and characters must not be altered
        var rawOwnerUserId = "Auth0|Custom_Provider-ID:XYZ_789";
        var username = new Username("developer");

        // Act
        var tenant = new Tenant(rawOwnerUserId, username);

        // Assert
        Assert.Equal(rawOwnerUserId, tenant.OwnerUserId);
    }

    [Fact]
    public void Tenant_ImplementsITenantEntity_AndTenantIdEqualsInheritedId()
    {
        // Arrange
        var tenant = new Tenant("user-123", new Username("adalovelace"));

        // Act & Assert
        Assert.IsAssignableFrom<ITenantEntity>(tenant);
        ITenantEntity tenantEntity = tenant;
        Assert.Equal(tenant.Id, tenantEntity.TenantId);
    }

    [Fact]
    public void UpdateUsername_WithNewValidUsername_UpdatesUsernameAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenant = new Tenant("user-123", new Username("ada-first"));
        var newUsername = new Username("ada-lovelace");
        Assert.Null(tenant.UpdatedAtUtc);

        // Act
        tenant.UpdateUsername(newUsername);

        // Assert
        Assert.Equal(newUsername, tenant.Username);
        Assert.NotNull(tenant.UpdatedAtUtc);
        Assert.True(tenant.UpdatedAtUtc >= tenant.CreatedAtUtc);
    }

    [Fact]
    public void UpdateUsername_PreservesIdAndOwnerUserId()
    {
        // Arrange
        var originalOwnerUserId = "user-identity-456";
        var tenant = new Tenant(originalOwnerUserId, new Username("initial-user"));
        var originalId = tenant.Id;
        var newUsername = new Username("updated-user");

        // Act
        tenant.UpdateUsername(newUsername);

        // Assert
        Assert.Equal(originalId, tenant.Id);
        Assert.Equal(originalOwnerUserId, tenant.OwnerUserId);
    }

    [Fact]
    public void UpdateUsername_WithDifferentInstanceHavingSameValue_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var username = new Username("adalovelace");
        var tenant = new Tenant("user-123", username);
        tenant.UpdateUsername(new Username("ada-lovelace"));
        var originalUpdatedAt = tenant.UpdatedAtUtc;

        // Act - re-apply same instance
        tenant.UpdateUsername(new Username("ada-lovelace"));

        // Assert
        Assert.Equal(originalUpdatedAt, tenant.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateUsername_WithDifferentInstanceHavingSameNormalizedValue_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenant = new Tenant("user-123", new Username("adalovelace"));
        tenant.UpdateUsername(new Username("ada-dev"));
        var originalUpdatedAt = tenant.UpdatedAtUtc;

        // Act - re-apply equivalent normalized value via different instance
        tenant.UpdateUsername(new Username("ADA-DEV"));

        // Assert
        Assert.Equal(originalUpdatedAt, tenant.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateUsername_WithNull_ThrowsArgumentNullException_AndDoesNotMutateState()
    {
        // Arrange
        var initialUsername = new Username("ada-lovelace");
        var tenant = new Tenant("user-123", initialUsername);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => tenant.UpdateUsername(null!));
        Assert.Equal("newUsername", exception.ParamName);
        Assert.Equal(initialUsername, tenant.Username);
        Assert.Null(tenant.UpdatedAtUtc);
    }
}
