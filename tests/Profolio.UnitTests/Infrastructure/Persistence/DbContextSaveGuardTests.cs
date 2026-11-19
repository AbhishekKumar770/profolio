namespace Profolio.UnitTests.Infrastructure.Persistence;

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Profolio.Domain.Common;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;
using Profolio.Infrastructure.Persistence;
using Xunit;

public sealed class DbContextSaveGuardTests
{
    private const string ConnectionString = "Host=127.0.0.1;Port=5432;Database=profolio_unit_test_dummy;Username=dummy;Password=dummy";

    private static DbContextOptions<ProfolioDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ProfolioDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
    }

    [Fact]
    public void SynchronousSaveChanges_ThrowsNotSupportedException()
    {
        var tenantId = Guid.NewGuid();
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(tenantId));

        Assert.Throws<NotSupportedException>(() => context.SaveChanges());
        Assert.Throws<NotSupportedException>(() => context.SaveChanges(true));
        Assert.Throws<NotSupportedException>(() => context.SaveChanges(false));
    }

    [Fact]
    public async Task SaveChangesAsync_WithoutTenantContext_ThrowsInvalidOperationException()
    {
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.Anonymous());
        var profile = new DeveloperProfile(Guid.NewGuid(), "Jane Doe", "Software Engineer");
        context.DeveloperProfiles.Add(profile);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Tenant context requires an authenticated user", ex.Message);
    }

    [Fact]
    public async Task SaveChangesAsync_AddingTenantInNormalScope_ThrowsInvalidOperationException()
    {
        var tenantId = Guid.NewGuid();
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(tenantId));
        var newTenant = new Tenant("user-999", new Username("johndoe"));
        context.Tenants.Add(newTenant);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Tenant creation is prohibited outside the dedicated provisioning scope", ex.Message);
    }

    [Fact]
    public async Task SaveChangesAsync_DeletingTenant_ThrowsInvalidOperationException()
    {
        var tenantId = Guid.NewGuid();
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(tenantId));
        var tenant = new Tenant("user-123", new Username("janedoe"));
        context.Entry(tenant).Property("Id").CurrentValue = tenantId;

        // Simulate deleting a tenant
        context.Tenants.Remove(tenant);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Tenant deletion is not supported in the MVP", ex.Message);
    }

    [Fact]
    public async Task SaveChangesAsync_CrossTenantEntityAdd_ThrowsInvalidOperationException()
    {
        var activeTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(activeTenantId));

        var skill = new Skill(otherTenantId, "C#", "Backend");
        context.Skills.Add(skill);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Cannot add entity with TenantId", ex.Message);
    }

    [Fact]
    public async Task SaveChangesAsync_TenantIdMutation_ThrowsInvalidOperationException()
    {
        var activeTenantId = Guid.NewGuid();
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(activeTenantId));

        var skill = new Skill(activeTenantId, "C#", "Backend");
        context.Skills.Attach(skill);

        // Attempt to reassign TenantId
        context.Entry(skill).Property(nameof(Skill.TenantId)).CurrentValue = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("TenantId is immutable and cannot be altered or reassigned", ex.Message);
    }

    [Fact]
    public async Task SaveChangesAsync_PortfolioVersion_ModificationOrDeletion_ThrowsInvalidOperationException()
    {
        var activeTenantId = Guid.NewGuid();
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(activeTenantId));

        var version = new PortfolioVersion(activeTenantId, 1, "{}");
        context.PortfolioVersions.Attach(version);

        // Test modification rejection
        context.Entry(version).State = EntityState.Modified;
        var modEx = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("PortfolioVersion is append-only and cannot be Modified", modEx.Message);

        // Test deletion rejection
        context.Entry(version).State = EntityState.Deleted;
        var delEx = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("PortfolioVersion is append-only and cannot be Deleted", delEx.Message);
    }

    [Fact]
    public async Task SaveChangesAsync_TwoPassValidation_PreservesTimestampsWhenValidationFails()
    {
        var activeTenantId = Guid.NewGuid();
        var initialTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new TestTimeProvider(initialTime);

        var context = new ProfolioDbContext(
            CreateOptions(),
            TestTenantContext.AuthenticatedWithTenant(activeTenantId),
            timeProvider);

        var validSkill = new Skill(activeTenantId, "C#");
        context.Skills.Attach(validSkill);
        context.Entry(validSkill).State = EntityState.Modified;
        var originalTimestamp = validSkill.UpdatedAtUtc;

        // Stage an invalid entity in the same change tracker
        var invalidSkill = new Skill(Guid.NewGuid(), "Rust");
        context.Skills.Add(invalidSkill); // Mismatched tenant

        // Advance time
        timeProvider.Advance(TimeSpan.FromHours(5));

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());

        // Invariant: Because Pass 1 failed, Pass 2 was never executed.
        // Timestamp of validSkill must remain unchanged.
        Assert.Equal(originalTimestamp, validSkill.UpdatedAtUtc);
    }

    [Fact]
    public async Task SaveChangesAsync_ModifyingTenantUsername_ThrowsInvalidOperationException()
    {
        var tenantId = Guid.NewGuid();
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(tenantId));
        var tenant = new Tenant("user-123", new Username("janedoe"));
        context.Entry(tenant).Property("Id").CurrentValue = tenantId;

        context.Tenants.Attach(tenant);
        tenant.UpdateUsername(new Username("newusername"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Tenant modification is prohibited in the MVP", ex.Message);
    }

    [Fact]
    public async Task SaveChangesAsync_ModifyingTenantOwnerUserId_ThrowsInvalidOperationException()
    {
        var tenantId = Guid.NewGuid();
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(tenantId));
        var tenant = new Tenant("user-123", new Username("janedoe"));
        context.Entry(tenant).Property("Id").CurrentValue = tenantId;

        context.Tenants.Attach(tenant);
        context.Entry(tenant).Property(nameof(Tenant.OwnerUserId)).CurrentValue = "user-999";

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Tenant modification is prohibited in the MVP", ex.Message);
    }

    [Fact]
    public void ModifyingTenantId_ThrowsInvalidOperationException_BeforeSaveChanges()
    {
        var tenantId = Guid.NewGuid();
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(tenantId));
        var tenant = new Tenant("user-123", new Username("janedoe"));
        context.Entry(tenant).Property("Id").CurrentValue = tenantId;

        context.Tenants.Attach(tenant);

        // Genuinely attempt to change the Tenant primary key using EF Core change tracker property entry.
        // EF Core change tracking itself strictly rejects primary key mutation on tracked entities upfront before persistence.
        var ex = Assert.Throws<InvalidOperationException>(() =>
            context.Entry(tenant).Property(nameof(Tenant.Id)).CurrentValue = Guid.NewGuid());

        Assert.Contains("part of a key and so cannot be modified", ex.Message);
    }

    [Fact]
    public async Task SaveChangesAsync_ModifyingTenantUpdatedAtUtc_ThrowsInvalidOperationException()
    {
        var tenantId = Guid.NewGuid();
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(tenantId));
        var tenant = new Tenant("user-123", new Username("janedoe"));
        context.Entry(tenant).Property("Id").CurrentValue = tenantId;

        context.Tenants.Attach(tenant);
        context.Entry(tenant).Property(nameof(Tenant.UpdatedAtUtc)).CurrentValue = DateTime.UtcNow;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Tenant modification is prohibited in the MVP", ex.Message);
    }

    [Fact]
    public async Task SaveChangesAsync_DeletingCrossTenantEntity_ThrowsInvalidOperationException()
    {
        var activeTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(activeTenantId));

        var skill = new Skill(otherTenantId, "C#", "Backend");
        context.Skills.Attach(skill);
        context.Skills.Remove(skill);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Cannot delete entity belonging to TenantId", ex.Message);
    }

    [Fact]
    public async Task SaveChangesAsync_ModifyingChildWithOriginalTenantIdMismatch_ThrowsInvalidOperationException()
    {
        var activeTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(activeTenantId));

        var skill = new Skill(otherTenantId, "C#", "Backend");
        context.Skills.Attach(skill);

        skill.UpdateDetails("F#", "Functional", 0);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("TenantId is immutable and cannot be altered or reassigned", ex.Message);
    }
}
