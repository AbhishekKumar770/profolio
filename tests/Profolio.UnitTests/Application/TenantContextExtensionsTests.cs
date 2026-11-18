using Profolio.Application.Abstractions.Tenancy;

namespace Profolio.UnitTests.Application;

public class TenantContextExtensionsTests
{
    private sealed class FakeTenantContext(Guid? tenantId, string? userId, bool isAuthenticated) : ITenantContext
    {
        public Guid? TenantId { get; } = tenantId;

        public string? UserId { get; } = userId;

        public bool IsAuthenticated { get; } = isAuthenticated;
    }

    [Fact]
    public void AnonymousState_HasNoTenantAndRejectsRequiredIdentifiers()
    {
        // Arrange
        var context = new FakeTenantContext(tenantId: null, userId: null, isAuthenticated: false);

        // Act & Assert
        Assert.False(context.IsAuthenticated);
        Assert.Null(context.UserId);
        Assert.Null(context.TenantId);
        Assert.False(context.HasTenant());
        Assert.Throws<InvalidOperationException>(() => context.GetRequiredTenantId());
        Assert.Throws<InvalidOperationException>(() => context.GetRequiredUserId());
    }

    [Fact]
    public void AuthenticatedWithoutTenantState_ReturnsRequiredUserIdAndRejectsRequiredTenantId()
    {
        // Arrange (onboarding state)
        var context = new FakeTenantContext(tenantId: null, userId: "user-123", isAuthenticated: true);

        // Act & Assert
        Assert.True(context.IsAuthenticated);
        Assert.False(context.HasTenant());
        Assert.Equal("user-123", context.GetRequiredUserId());
        Assert.Throws<InvalidOperationException>(() => context.GetRequiredTenantId());
    }

    [Fact]
    public void AuthenticatedWithTenantState_HasTenantAndReturnsRequiredIdentifiers()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var context = new FakeTenantContext(tenantId: tenantId, userId: "user-123", isAuthenticated: true);

        // Act & Assert
        Assert.True(context.IsAuthenticated);
        Assert.True(context.HasTenant());
        Assert.Equal(tenantId, context.GetRequiredTenantId());
        Assert.Equal("user-123", context.GetRequiredUserId());
    }

    [Fact]
    public void UnauthenticatedWithPopulatedTenantId_HasNoTenantAndRejectsRequiredIdentifiers()
    {
        // Arrange - defensive security test: populated TenantId without authentication must fail closed
        var context = new FakeTenantContext(tenantId: Guid.NewGuid(), userId: null, isAuthenticated: false);

        // Act & Assert
        Assert.False(context.HasTenant());
        var tenantException = Assert.Throws<InvalidOperationException>(() => context.GetRequiredTenantId());
        Assert.Contains("authenticated", tenantException.Message, StringComparison.OrdinalIgnoreCase);

        var userException = Assert.Throws<InvalidOperationException>(() => context.GetRequiredUserId());
        Assert.Contains("authenticated", userException.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnauthenticatedWithPopulatedUserIdAndTenantId_HasNoTenantAndRejectsRequiredIdentifiers()
    {
        // Arrange
        var context = new FakeTenantContext(tenantId: Guid.NewGuid(), userId: "user-123", isAuthenticated: false);

        // Act & Assert
        Assert.False(context.HasTenant());
        Assert.Throws<InvalidOperationException>(() => context.GetRequiredTenantId());
        Assert.Throws<InvalidOperationException>(() => context.GetRequiredUserId());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void AuthenticatedWithMissingOrWhitespaceUserId_HasNoTenantAndRejectsRequiredIdentifiers(string? invalidUserId)
    {
        // Arrange
        var context = new FakeTenantContext(tenantId: Guid.NewGuid(), userId: invalidUserId, isAuthenticated: true);

        // Act & Assert
        Assert.False(context.HasTenant());
        var tenantException = Assert.Throws<InvalidOperationException>(() => context.GetRequiredTenantId());
        Assert.Contains("user ID", tenantException.Message, StringComparison.OrdinalIgnoreCase);

        var userException = Assert.Throws<InvalidOperationException>(() => context.GetRequiredUserId());
        Assert.Contains("user ID", userException.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AuthenticatedWithEmptyTenantGuid_HasNoTenantAndRejectsRequiredTenantId()
    {
        // Arrange
        var context = new FakeTenantContext(tenantId: Guid.Empty, userId: "user-123", isAuthenticated: true);

        // Act & Assert
        Assert.False(context.HasTenant());
        Assert.Equal("user-123", context.GetRequiredUserId());
        var exception = Assert.Throws<InvalidOperationException>(() => context.GetRequiredTenantId());
        Assert.Contains("Tenant ID", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NullContext_ThrowsArgumentNullExceptionForAllExtensionMethods()
    {
        // Arrange
        ITenantContext context = null!;

        // Act & Assert
        var hasTenantEx = Assert.Throws<ArgumentNullException>(() => context.HasTenant());
        Assert.Equal("tenantContext", hasTenantEx.ParamName);

        var tenantIdEx = Assert.Throws<ArgumentNullException>(() => context.GetRequiredTenantId());
        Assert.Equal("tenantContext", tenantIdEx.ParamName);

        var userIdEx = Assert.Throws<ArgumentNullException>(() => context.GetRequiredUserId());
        Assert.Equal("tenantContext", userIdEx.ParamName);
    }
}
