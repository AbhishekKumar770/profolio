namespace Profolio.Infrastructure.Persistence;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Profolio.Application.Abstractions.Persistence;
using Profolio.Application.Abstractions.Tenancy;
using Profolio.Domain.Common;
using Profolio.Domain.Entities;

public sealed class ProfolioDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly TimeProvider _timeProvider;
    private ProvisioningState? _activeProvisioningState;

    public ProfolioDbContext(
        DbContextOptions<ProfolioDbContext> options,
        ITenantContext tenantContext,
        TimeProvider? timeProvider = null)
        : base(options)
    {
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<DeveloperProfile> DeveloperProfiles => Set<DeveloperProfile>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();
    public DbSet<PortfolioConfiguration> PortfolioConfigurations => Set<PortfolioConfiguration>();
    public DbSet<PortfolioVersion> PortfolioVersions => Set<PortfolioVersion>();
    public DbSet<PublishingState> PublishingStates => Set<PublishingState>();

    // -------------------------------------------------------------------------
    // Context Properties for EF Core Query Filter Translation
    // -------------------------------------------------------------------------
    public bool HasActiveTenant =>
        _tenantContext.IsAuthenticated &&
        !string.IsNullOrWhiteSpace(_tenantContext.UserId) &&
        _tenantContext.TenantId.HasValue &&
        _tenantContext.TenantId.Value != Guid.Empty;

    public Guid CurrentTenantId => _tenantContext.TenantId ?? Guid.Empty;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProfolioDbContext).Assembly);

        // Apply Named Global Query Filters ("TenantFilter")
        modelBuilder.Entity<Tenant>()
            .HasQueryFilter("TenantFilter", t => HasActiveTenant && t.Id == CurrentTenantId);

        modelBuilder.Entity<DeveloperProfile>()
            .HasQueryFilter("TenantFilter", p => HasActiveTenant && p.TenantId == CurrentTenantId);

        modelBuilder.Entity<Skill>()
            .HasQueryFilter("TenantFilter", s => HasActiveTenant && s.TenantId == CurrentTenantId);

        modelBuilder.Entity<WorkExperience>()
            .HasQueryFilter("TenantFilter", w => HasActiveTenant && w.TenantId == CurrentTenantId);

        modelBuilder.Entity<Education>()
            .HasQueryFilter("TenantFilter", e => HasActiveTenant && e.TenantId == CurrentTenantId);

        modelBuilder.Entity<Project>()
            .HasQueryFilter("TenantFilter", p => HasActiveTenant && p.TenantId == CurrentTenantId);

        modelBuilder.Entity<SocialLink>()
            .HasQueryFilter("TenantFilter", s => HasActiveTenant && s.TenantId == CurrentTenantId);

        modelBuilder.Entity<PortfolioConfiguration>()
            .HasQueryFilter("TenantFilter", c => HasActiveTenant && c.TenantId == CurrentTenantId);

        modelBuilder.Entity<PortfolioVersion>()
            .HasQueryFilter("TenantFilter", v => HasActiveTenant && v.TenantId == CurrentTenantId);

        modelBuilder.Entity<PublishingState>()
            .HasQueryFilter("TenantFilter", p => HasActiveTenant && p.TenantId == CurrentTenantId);
    }

    // -------------------------------------------------------------------------
    // Synchronous Save Overloads: Strictly Prohibited
    // -------------------------------------------------------------------------
    public override int SaveChanges() =>
        throw new NotSupportedException("Synchronous persistence is prohibited in Profolio. Use SaveChangesAsync instead.");

    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        throw new NotSupportedException("Synchronous persistence is prohibited in Profolio. Use SaveChangesAsync instead.");

    // -------------------------------------------------------------------------
    // Asynchronous Save Overloads: Centralized Validation Routing
    // -------------------------------------------------------------------------
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        var wasProvisioning = _activeProvisioningState != null;

        try
        {
            ValidateTenantBoundaryAndApplyTimestamps();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        finally
        {
            if (wasProvisioning)
            {
                _activeProvisioningState = null;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Centralized Two-Pass Security Guard
    // -------------------------------------------------------------------------
    private void ValidateTenantBoundaryAndApplyTimestamps()
    {
        if (_activeProvisioningState != null)
        {
            ValidateProvisioningScope(_activeProvisioningState);
            return;
        }

        var activeTenantId = _tenantContext.GetRequiredTenantId();
        var trackedEntries = ChangeTracker.Entries().ToList();

        // PASS 1: Strict Validation (Zero Change Tracker Mutations)
        foreach (var entry in trackedEntries)
        {
            if (entry.Entity is Tenant)
            {
                if (entry.State == EntityState.Added)
                {
                    throw new InvalidOperationException("Tenant creation is prohibited outside the dedicated provisioning scope.");
                }

                if (entry.State == EntityState.Modified)
                {
                    throw new InvalidOperationException("Tenant modification is prohibited in the MVP.");
                }

                if (entry.State == EntityState.Deleted)
                {
                    throw new InvalidOperationException("Tenant deletion is not supported in the MVP.");
                }
            }
            else if (entry.Entity is PortfolioVersion)
            {
                if (entry.State is EntityState.Modified or EntityState.Deleted)
                {
                    throw new InvalidOperationException($"PortfolioVersion is append-only and cannot be {entry.State}.");
                }

                if (entry.State == EntityState.Added)
                {
                    var addedTenantId = (Guid)entry.Property(nameof(PortfolioVersion.TenantId)).CurrentValue!;
                    if (addedTenantId != activeTenantId)
                    {
                        throw new InvalidOperationException(
                            $"Cannot add PortfolioVersion with TenantId '{addedTenantId}' under active tenant '{activeTenantId}'.");
                    }
                }
            }
            else if (entry.Entity is ITenantEntity)
            {
                var tenantIdProp = entry.Property(nameof(ITenantEntity.TenantId));

                switch (entry.State)
                {
                    case EntityState.Added:
                        var addedTenantId = (Guid)tenantIdProp.CurrentValue!;
                        if (addedTenantId != activeTenantId)
                        {
                            throw new InvalidOperationException(
                                $"Cannot add entity with TenantId '{addedTenantId}' under active tenant '{activeTenantId}'.");
                        }
                        break;

                    case EntityState.Modified:
                        var originalTenantId = (Guid)tenantIdProp.OriginalValue!;
                        var currentTenantId = (Guid)tenantIdProp.CurrentValue!;

                        if (originalTenantId != activeTenantId || currentTenantId != activeTenantId || tenantIdProp.IsModified)
                        {
                            throw new InvalidOperationException("TenantId is immutable and cannot be altered or reassigned.");
                        }
                        break;

                    case EntityState.Deleted:
                        var deletedTenantId = (Guid)tenantIdProp.OriginalValue!;
                        if (deletedTenantId != activeTenantId)
                        {
                            throw new InvalidOperationException(
                                $"Cannot delete entity belonging to TenantId '{deletedTenantId}' from active tenant '{activeTenantId}'.");
                        }
                        break;
                }
            }
        }

        // PASS 2: Timestamp Application (Executed ONLY after all validations succeed)
        var currentUtc = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in trackedEntries)
        {
            if (entry.Entity is BaseEntity && entry.State == EntityState.Modified)
            {
                entry.Property(nameof(BaseEntity.UpdatedAtUtc)).CurrentValue = currentUtc;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Internal Provisioning Scope Implementation
    // -------------------------------------------------------------------------
    internal IDisposable StartProvisioningScope(Guid serverGeneratedTenantId)
    {
        if (serverGeneratedTenantId == Guid.Empty)
        {
            throw new ArgumentException("Server-generated tenant ID cannot be empty.", nameof(serverGeneratedTenantId));
        }

        if (_activeProvisioningState != null)
        {
            throw new InvalidOperationException("Nested provisioning scopes are prohibited.");
        }

        var authenticatedUserId = _tenantContext.GetRequiredUserId();

        _activeProvisioningState = new ProvisioningState(serverGeneratedTenantId, authenticatedUserId);
        return new ProvisioningScopeToken(this);
    }

    private void ValidateProvisioningScope(ProvisioningState state)
    {
        var entries = ChangeTracker.Entries().ToList();

        foreach (var entry in entries)
        {
            if (entry.State != EntityState.Added)
            {
                throw new InvalidOperationException(
                    $"Provisioning scope strictly permits ONLY Added entities, but found entity in state '{entry.State}'.");
            }
        }

        if (entries.Count != 4)
        {
            throw new InvalidOperationException(
                $"Tenant provisioning requires exactly 4 initial aggregate roots, but {entries.Count} were tracked.");
        }

        Tenant? provisionedTenant = null;
        DeveloperProfile? provisionedProfile = null;
        PortfolioConfiguration? provisionedConfig = null;
        PublishingState? provisionedPubState = null;

        foreach (var entry in entries)
        {
            switch (entry.Entity)
            {
                case Tenant tenant:
                    if (provisionedTenant != null)
                    {
                        throw new InvalidOperationException("Duplicate Tenant entity tracked during provisioning.");
                    }
                    provisionedTenant = tenant;
                    break;

                case DeveloperProfile profile:
                    if (provisionedProfile != null)
                    {
                        throw new InvalidOperationException("Duplicate DeveloperProfile entity tracked during provisioning.");
                    }
                    provisionedProfile = profile;
                    break;

                case PortfolioConfiguration config:
                    if (provisionedConfig != null)
                    {
                        throw new InvalidOperationException("Duplicate PortfolioConfiguration entity tracked during provisioning.");
                    }
                    provisionedConfig = config;
                    break;

                case PublishingState pubState:
                    if (provisionedPubState != null)
                    {
                        throw new InvalidOperationException("Duplicate PublishingState entity tracked during provisioning.");
                    }
                    provisionedPubState = pubState;
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Entity type '{entry.Entity.GetType().Name}' is not permitted during initial tenant onboarding.");
            }
        }

        if (provisionedTenant == null || provisionedProfile == null ||
            provisionedConfig == null || provisionedPubState == null)
        {
            throw new InvalidOperationException(
                "Initial tenant onboarding requires Tenant, DeveloperProfile, PortfolioConfiguration, and PublishingState.");
        }

        if (provisionedTenant.Id != state.DesignatedTenantId)
        {
            throw new InvalidOperationException(
                $"Tenant.Id '{provisionedTenant.Id}' does not match designated tenant ID '{state.DesignatedTenantId}'.");
        }

        if (!string.Equals(provisionedTenant.OwnerUserId, state.DesignatedOwnerUserId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Tenant.OwnerUserId '{provisionedTenant.OwnerUserId}' does not match authenticated user ID '{state.DesignatedOwnerUserId}'.");
        }

        if (provisionedProfile.TenantId != state.DesignatedTenantId)
        {
            throw new InvalidOperationException("DeveloperProfile.TenantId does not match designated tenant ID.");
        }

        if (provisionedConfig.TenantId != state.DesignatedTenantId)
        {
            throw new InvalidOperationException("PortfolioConfiguration.TenantId does not match designated tenant ID.");
        }

        if (provisionedPubState.TenantId != state.DesignatedTenantId)
        {
            throw new InvalidOperationException("PublishingState.TenantId does not match designated tenant ID.");
        }
    }

    private sealed record ProvisioningState(Guid DesignatedTenantId, string DesignatedOwnerUserId);

    private sealed class ProvisioningScopeToken : IDisposable
    {
        private ProfolioDbContext? _context;

        public ProvisioningScopeToken(ProfolioDbContext context) => _context = context;

        public void Dispose()
        {
            if (_context != null)
            {
                _context._activeProvisioningState = null;
                _context = null;
            }
        }
    }
}
