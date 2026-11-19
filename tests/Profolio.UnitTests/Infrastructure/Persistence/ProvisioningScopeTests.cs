namespace Profolio.UnitTests.Infrastructure.Persistence;

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;
using Profolio.Infrastructure.Persistence;
using Xunit;

public sealed class ProvisioningScopeTests
{
    private const string ConnectionString = "Host=127.0.0.1;Port=5432;Database=profolio_unit_test_dummy;Username=dummy;Password=dummy";

    private static DbContextOptions<ProfolioDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ProfolioDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
    }

    [Fact]
    public void StartProvisioningScope_WithEmptyTenantId_ThrowsArgumentException()
    {
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedNoTenant("user-1"));
        Assert.Throws<ArgumentException>(() => context.StartProvisioningScope(Guid.Empty));
    }

    [Fact]
    public void StartProvisioningScope_WhenUnauthenticated_ThrowsInvalidOperationException()
    {
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.Anonymous());
        Assert.Throws<InvalidOperationException>(() => context.StartProvisioningScope(Guid.NewGuid()));
    }

    [Fact]
    public void StartProvisioningScope_WhenNested_ThrowsInvalidOperationException()
    {
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedNoTenant("user-1"));
        using var scope1 = context.StartProvisioningScope(Guid.NewGuid());

        var ex = Assert.Throws<InvalidOperationException>(() => context.StartProvisioningScope(Guid.NewGuid()));
        Assert.Contains("Nested provisioning scopes are prohibited", ex.Message);
    }

    [Fact]
    public void ProvisioningScopeToken_DisposalIsIdempotent()
    {
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedNoTenant("user-1"));
        var scope = context.StartProvisioningScope(Guid.NewGuid());

        scope.Dispose();
        var ex = Record.Exception(() => { scope.Dispose(); });
        Assert.Null(ex);
    }

    [Fact]
    public async Task Provisioning_WithMissingEntity_ThrowsInvalidOperationException()
    {
        var tenantId = Guid.NewGuid();
        var userId = "user-100";
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedNoTenant(userId));

        using var scope = context.StartProvisioningScope(tenantId);

        // Add only 3 entities (omit PublishingState)
        context.Tenants.Add(new Tenant(userId, new Username($"user{Guid.NewGuid():N}"[..12])));
        context.Entry(context.Tenants.Local.First()).Property("Id").CurrentValue = tenantId;
        context.DeveloperProfiles.Add(new DeveloperProfile(tenantId, "John Doe", "Engineer"));
        context.PortfolioConfigurations.Add(new PortfolioConfiguration(tenantId));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Tenant provisioning requires exactly 4 initial aggregate roots", ex.Message);
    }

    [Fact]
    public async Task Provisioning_WithDuplicateEntity_ThrowsInvalidOperationException()
    {
        var tenantId = Guid.NewGuid();
        var userId = "user-101";
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedNoTenant(userId));

        using var scope = context.StartProvisioningScope(tenantId);

        var tenant = new Tenant(userId, new Username($"user{Guid.NewGuid():N}"[..12]));
        context.Entry(tenant).Property("Id").CurrentValue = tenantId;
        context.Tenants.Add(tenant);

        // Duplicate DeveloperProfile
        context.DeveloperProfiles.Add(new DeveloperProfile(tenantId, "Profile 1", "Headline"));
        context.DeveloperProfiles.Add(new DeveloperProfile(tenantId, "Profile 2", "Headline"));
        context.PortfolioConfigurations.Add(new PortfolioConfiguration(tenantId));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Duplicate DeveloperProfile entity tracked during provisioning", ex.Message);
    }

    [Fact]
    public async Task Provisioning_WithUnsupportedEntity_ThrowsInvalidOperationException()
    {
        var tenantId = Guid.NewGuid();
        var userId = "user-102";
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedNoTenant(userId));

        using var scope = context.StartProvisioningScope(tenantId);

        var tenant = new Tenant(userId, new Username($"user{Guid.NewGuid():N}"[..12]));
        context.Entry(tenant).Property("Id").CurrentValue = tenantId;
        context.Tenants.Add(tenant);

        context.DeveloperProfiles.Add(new DeveloperProfile(tenantId, "Profile", "Headline"));
        context.PortfolioConfigurations.Add(new PortfolioConfiguration(tenantId));
        // Add Project (unsupported during provisioning)
        context.Projects.Add(new Project(tenantId, "My Project"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Entity type 'Project' is not permitted during initial tenant onboarding", ex.Message);
    }

    [Fact]
    public async Task Provisioning_WithMismatchedTenantOwner_ThrowsInvalidOperationException()
    {
        var tenantId = Guid.NewGuid();
        var authenticatedUser = "auth-user";
        var wrongOwner = "different-user";
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedNoTenant(authenticatedUser));

        using var scope = context.StartProvisioningScope(tenantId);

        var tenant = new Tenant(wrongOwner, new Username($"user{Guid.NewGuid():N}"[..12]));
        context.Entry(tenant).Property("Id").CurrentValue = tenantId;
        context.Tenants.Add(tenant);

        context.DeveloperProfiles.Add(new DeveloperProfile(tenantId, "Profile", "Headline"));
        context.PortfolioConfigurations.Add(new PortfolioConfiguration(tenantId));
        context.PublishingStates.Add(new PublishingState(tenantId));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("does not match authenticated user ID", ex.Message);
    }

    [Fact]
    public async Task Provisioning_FailureRevokesPrivilegeForSubsequentSaves()
    {
        var tenantId = Guid.NewGuid();
        var userId = "user-revocation";
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedNoTenant(userId));

        using (var scope = context.StartProvisioningScope(tenantId))
        {
            // Invalid entity configuration that fails provisioning validation
            context.Tenants.Add(new Tenant("wrong-user", new Username($"user{Guid.NewGuid():N}"[..12])));

            // First save attempt fails
            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());

            // Even if the caller catches the exception within the same using block,
            // provisioning privilege is immediately revoked. A second save must execute as standard save
            // and fail closed because context has no active tenant.
            var secondEx = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
            Assert.Contains("Tenant ID is not resolved in the current context", secondEx.Message);
        }
    }
}
