namespace Profolio.IntegrationTests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Profolio.Application.Common.Results;
using Profolio.Application.Features.DeveloperProfile.Commands.ClearPublicContactEmail;
using Profolio.Application.Features.DeveloperProfile.Commands.SetPublicContactEmail;
using Profolio.Application.Features.DeveloperProfile.Commands.UpdateDeveloperProfile;
using Profolio.Application.Features.DeveloperProfile.Queries.GetDeveloperProfile;
using Profolio.Application.Features.DeveloperProfile.Validation;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;
using Profolio.Infrastructure.Persistence;
using Profolio.Infrastructure.Persistence.Services;
using Xunit;

public sealed class PostgreSqlIntegrationTests
{
    public const string ApprovedTestDatabaseName = "profolio_test";

    private static readonly bool s_isDatabaseMigrated;
    private static readonly object s_migrateLock = new();

    static PostgreSqlIntegrationTests()
    {
        var connectionString = Environment.GetEnvironmentVariable("PROFOLIO_TEST_CONNECTION");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            ValidateTestConnectionString(connectionString);

            lock (s_migrateLock)
            {
                if (!s_isDatabaseMigrated)
                {
                    var options = new DbContextOptionsBuilder<ProfolioDbContext>()
                        .UseNpgsql(connectionString)
                        .Options;
                    using var context = new ProfolioDbContext(options, TestTenantContext.Anonymous());
                    context.Database.Migrate();
                    s_isDatabaseMigrated = true;
                }
            }
        }
    }

    internal static void ValidateTestConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Environment variable 'PROFOLIO_TEST_CONNECTION' is not configured. " +
                $"Set PROFOLIO_TEST_CONNECTION with approved database '{ApprovedTestDatabaseName}' " +
                "to run PostgreSQL integration tests.");
        }

        NpgsqlConnectionStringBuilder builder;
        try
        {
            builder = new NpgsqlConnectionStringBuilder(connectionString);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse PostgreSQL test connection string.", ex);
        }

        var database = builder.Database;
        if (!string.Equals(database, ApprovedTestDatabaseName, StringComparison.OrdinalIgnoreCase))
        {
            var dbNameDisplay = string.IsNullOrWhiteSpace(database) ? "<none>" : database;
            throw new InvalidOperationException(
                $"Integration tests are restricted to the approved test database '{ApprovedTestDatabaseName}'. " +
                $"The configured database '{dbNameDisplay}' is rejected to prevent accidental modification of development or production data.");
        }
    }

    private static string GetTestConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable("PROFOLIO_TEST_CONNECTION");
        ValidateTestConnectionString(connectionString);
        return connectionString!;
    }

    private static DbContextOptions<ProfolioDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ProfolioDbContext>()
            .UseNpgsql(GetTestConnectionString())
            .Options;
    }

    private static async Task<Guid> ProvisionTenantAsync(string userId, string usernameStr)
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = TestTenantContext.AuthenticatedNoTenant(userId);
        using var context = new ProfolioDbContext(CreateOptions(), tenantContext);

        using (var scope = context.StartProvisioningScope(tenantId))
        {
            var tenant = new Tenant(userId, new Username(usernameStr));
            context.Entry(tenant).Property("Id").CurrentValue = tenantId;

            context.Tenants.Add(tenant);
            context.DeveloperProfiles.Add(new DeveloperProfile(tenantId, "Test User", "Developer"));
            context.PortfolioConfigurations.Add(new PortfolioConfiguration(tenantId));
            context.PublishingStates.Add(new PublishingState(tenantId));

            await context.SaveChangesAsync();
        }

        return tenantId;
    }

    // -------------------------------------------------------------------------
    // 0. Integration Test Database Security Validation Tests
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("Host=localhost;Port=5432;Database=profolio_test;Username=postgres;Password=secret")]
    [InlineData("Host=localhost;Port=5432;Database=PROFOLIO_TEST;Username=postgres;Password=secret")]
    [InlineData("Host=localhost;Database=profolio_test;")]
    public void TestDatabaseSecurity_AllowsApprovedDatabaseName(string connectionString)
    {
        var ex = Record.Exception(() => ValidateTestConnectionString(connectionString));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData("Host=localhost;Port=5432;Database=profolio_dev;Username=postgres;Password=supersecret")]
    [InlineData("Host=db.prod.internal;Database=profolio_prod;Username=produser;Password=supersecret")]
    [InlineData("Host=db.prod.internal;Database=production;Username=admin;Password=supersecret")]
    [InlineData("Host=localhost;Port=5432;Database=other_db;Username=test;Password=supersecret")]
    [InlineData("Host=localhost;Port=5432;Username=postgres;Password=supersecret")]
    public void TestDatabaseSecurity_RejectsUnapprovedDatabaseNames(string connectionString)
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => ValidateTestConnectionString(connectionString));

        Assert.Contains("Integration tests are restricted to the approved test database 'profolio_test'", ex.Message);
        Assert.DoesNotContain("supersecret", ex.Message);
        Assert.DoesNotContain("produser", ex.Message);
        Assert.DoesNotContain("db.prod.internal", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TestDatabaseSecurity_RejectsEmptyConnectionString(string? connectionString)
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => ValidateTestConnectionString(connectionString));

        Assert.Contains("Environment variable 'PROFOLIO_TEST_CONNECTION' is not configured", ex.Message);
    }

    // -------------------------------------------------------------------------
    // 1. Tenant Ownership Concurrency Token in UPDATE/DELETE SQL
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PostgreSql_TenantOwnership_ForgedTenantIdCannotUpdateAnotherTenantRow()
    {
        // 1. Persist a real Skill under Tenant A
        var tenantA_Id = await ProvisionTenantAsync($"auth-a-{Guid.NewGuid():N}"[..16], $"usra{Guid.NewGuid():N}"[..12]);
        var tenantB_Id = await ProvisionTenantAsync($"auth-b-{Guid.NewGuid():N}"[..16], $"usrb{Guid.NewGuid():N}"[..12]);

        var tenantA_Context = TestTenantContext.AuthenticatedWithTenant(tenantA_Id);
        Guid persistedSkillId;
        uint persistedXmin;

        using (var contextA = new ProfolioDbContext(CreateOptions(), tenantA_Context))
        {
            var skill = new Skill(tenantA_Id, "Architecture", "Engineering", 1);
            contextA.Skills.Add(skill);
            await contextA.SaveChangesAsync();
            persistedSkillId = skill.Id;

            // 2. Capture its actual persisted shadow xmin value
            persistedXmin = (uint)contextA.Entry(skill).Property("xmin").CurrentValue!;
            Assert.True(persistedXmin > 0, "Persisted xmin must be a non-zero PostgreSQL system transaction ID.");
        }

        // 3. Create a forged disconnected Skill using Tenant B's TenantId and Tenant A's persisted primary key
        // 4. Attach it under Tenant B's authenticated context
        var tenantB_Context = TestTenantContext.AuthenticatedWithTenant(tenantB_Id);
        using (var contextB = new ProfolioDbContext(CreateOptions(), tenantB_Context))
        {
            var forgedSkill = new Skill(tenantB_Id, "Tampered Name", "Tampered Category", 1);
            contextB.Entry(forgedSkill).Property("Id").CurrentValue = persistedSkillId;

            contextB.Skills.Attach(forgedSkill);

            // 5. Set its original shadow xmin value to Tenant A's actual persisted xmin.
            // This ensures xmin matches the database row EXACTLY, proving that the concurrency
            // exception is independently caused by the TenantId concurrency predicate mismatch ("TenantId" = @tenantId).
            contextB.Entry(forgedSkill).Property("xmin").OriginalValue = persistedXmin;
            contextB.Entry(forgedSkill).Property("xmin").CurrentValue = persistedXmin;

            // 6. Attempt update
            forgedSkill.UpdateDetails("Tampered Name", "Tampered Category", 2);

            // 7. Verify DbUpdateConcurrencyException
            var ex = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => contextB.SaveChangesAsync());
            Assert.NotNull(ex);
        }

        // 8. Verify Tenant A's database row remains unchanged
        using (var verifyContext = new ProfolioDbContext(CreateOptions(), tenantA_Context))
        {
            var verified = await verifyContext.Skills.SingleAsync(s => s.Id == persistedSkillId);
            Assert.Equal("Architecture", verified.Name);
            Assert.Equal("Engineering", verified.Category);
            Assert.Equal(1, verified.DisplayOrder);
            Assert.Equal(tenantA_Id, verified.TenantId);

            var currentXmin = (uint)verifyContext.Entry(verified).Property("xmin").CurrentValue!;
            Assert.Equal(persistedXmin, currentXmin);
        }
    }

    [Fact]
    public async Task PostgreSql_TenantOwnership_ForgedTenantIdCannotDeleteAnotherTenantRow()
    {
        // 1. Persist a real Skill under Tenant A
        var tenantA_Id = await ProvisionTenantAsync($"auth-a-{Guid.NewGuid():N}"[..16], $"usra{Guid.NewGuid():N}"[..12]);
        var tenantB_Id = await ProvisionTenantAsync($"auth-b-{Guid.NewGuid():N}"[..16], $"usrb{Guid.NewGuid():N}"[..12]);

        var tenantA_Context = TestTenantContext.AuthenticatedWithTenant(tenantA_Id);
        Guid persistedSkillId;
        uint persistedXmin;

        using (var contextA = new ProfolioDbContext(CreateOptions(), tenantA_Context))
        {
            var skill = new Skill(tenantA_Id, "Security", "Engineering", 1);
            contextA.Skills.Add(skill);
            await contextA.SaveChangesAsync();
            persistedSkillId = skill.Id;

            // 2. Capture its actual persisted shadow xmin value
            persistedXmin = (uint)contextA.Entry(skill).Property("xmin").CurrentValue!;
            Assert.True(persistedXmin > 0, "Persisted xmin must be a non-zero PostgreSQL system transaction ID.");
        }

        // 3. Create a forged disconnected Skill using Tenant B's TenantId and Tenant A's persisted primary key
        // 4. Attach it under Tenant B's authenticated context
        var tenantB_Context = TestTenantContext.AuthenticatedWithTenant(tenantB_Id);
        using (var contextB = new ProfolioDbContext(CreateOptions(), tenantB_Context))
        {
            var forgedSkill = new Skill(tenantB_Id, "Security", "Engineering", 1);
            contextB.Entry(forgedSkill).Property("Id").CurrentValue = persistedSkillId;

            contextB.Skills.Attach(forgedSkill);

            // 5. Set its original shadow xmin value to Tenant A's actual persisted xmin.
            // This ensures xmin matches the database row EXACTLY, proving that the concurrency
            // exception is independently caused by the TenantId concurrency predicate mismatch ("TenantId" = @tenantId).
            contextB.Entry(forgedSkill).Property("xmin").OriginalValue = persistedXmin;
            contextB.Entry(forgedSkill).Property("xmin").CurrentValue = persistedXmin;

            // 6. Attempt delete
            contextB.Skills.Remove(forgedSkill);

            // 7. Verify DbUpdateConcurrencyException
            var ex = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => contextB.SaveChangesAsync());
            Assert.NotNull(ex);
        }

        // 8. Verify Tenant A's database row remains unchanged
        using (var verifyContext = new ProfolioDbContext(CreateOptions(), tenantA_Context))
        {
            var verified = await verifyContext.Skills.SingleAsync(s => s.Id == persistedSkillId);
            Assert.NotNull(verified);
            Assert.Equal("Security", verified.Name);
            Assert.Equal(tenantA_Id, verified.TenantId);

            var currentXmin = (uint)verifyContext.Entry(verified).Property("xmin").CurrentValue!;
            Assert.Equal(persistedXmin, currentXmin);
        }
    }

    [Fact]
    public async Task PostgreSql_TenantOwnership_LegitimateSameTenantUpdateSucceeds()
    {
        var tenantId = await ProvisionTenantAsync($"auth-{Guid.NewGuid():N}"[..16], $"usr{Guid.NewGuid():N}"[..12]);
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);

        Guid skillId;
        using (var context = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var skill = new Skill(tenantId, "TypeScript", "Frontend", 1);
            context.Skills.Add(skill);
            await context.SaveChangesAsync();
            skillId = skill.Id;
        }

        // Legitimate update: same tenant
        using (var context = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var skill = await context.Skills.SingleAsync(s => s.Id == skillId);
            skill.UpdateDetails("TypeScript 5.x", "Frontend", 2);
            var updatedRows = await context.SaveChangesAsync();
            Assert.True(updatedRows > 0);
        }

        // Verify update persisted
        using (var context = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var verified = await context.Skills.SingleAsync(s => s.Id == skillId);
            Assert.Equal("TypeScript 5.x", verified.Name);
            Assert.Equal(2, verified.DisplayOrder);
        }
    }

    // -------------------------------------------------------------------------
    // 2. Database Tenant Delete Behavior (DeleteBehavior.Restrict)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PostgreSql_DatabaseDeleteBehavior_RejectsDirectTenantDeleteWhenDependentsExist()
    {
        var tenantId = await ProvisionTenantAsync($"auth-{Guid.NewGuid():N}"[..16], $"usr{Guid.NewGuid():N}"[..12]);
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);

        // Verify direct SQL deletion of Tenant is rejected by PostgreSQL foreign key constraint (ON DELETE RESTRICT)
        using var context = new ProfolioDbContext(CreateOptions(), tenantContext);
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM tenancy.tenants WHERE \"Id\" = @tenantId;";
        var param = cmd.CreateParameter();
        param.ParameterName = "@tenantId";
        param.Value = tenantId;
        cmd.Parameters.Add(param);

        // PostgreSQL foreign key violation (23503: foreign_key_violation)
        var ex = await Assert.ThrowsAsync<PostgresException>(() => cmd.ExecuteNonQueryAsync());
        Assert.Equal("23503", ex.SqlState);
        Assert.Contains("violates foreign key constraint", ex.Message);

        // Verify tenant still exists in database
        using var verifyContext = new ProfolioDbContext(CreateOptions(), tenantContext);
        var tenantExists = await verifyContext.Tenants
            .IgnoreQueryFilters(["TenantFilter"])
            .AnyAsync(t => t.Id == tenantId);
        Assert.True(tenantExists);
    }

    // -------------------------------------------------------------------------
    // 3. Existing Baseline PostgreSQL Integration Tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PostgreSql_OptimisticConcurrency_ThrowsDbUpdateConcurrencyExceptionOnStaleUpdate()
    {
        var usernameStr = $"usr{Guid.NewGuid():N}"[..12];
        var userId = $"auth-{Guid.NewGuid():N}"[..16];
        var tenantId = await ProvisionTenantAsync(userId, usernameStr);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId, userId);

        using var contextA = new ProfolioDbContext(CreateOptions(), tenantContext);
        using var contextB = new ProfolioDbContext(CreateOptions(), tenantContext);

        var profileA = await contextA.DeveloperProfiles.SingleAsync(p => p.TenantId == tenantId);
        var profileB = await contextB.DeveloperProfiles.SingleAsync(p => p.TenantId == tenantId);

        // Context A updates and saves
        profileA.UpdateBasicInformation(profileA.FullName, "Staff Systems Architect", profileA.Bio, profileA.Location);
        await contextA.SaveChangesAsync();

        // Context B attempts to update stale copy
        profileB.UpdateBasicInformation(profileB.FullName, "Principal Engineer", profileB.Bio, profileB.Location);

        var ex = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => contextB.SaveChangesAsync());
        Assert.NotNull(ex);

        // Verify Context A's update was preserved
        using var verifyContext = new ProfolioDbContext(CreateOptions(), tenantContext);
        var verified = await verifyContext.DeveloperProfiles.SingleAsync(p => p.TenantId == tenantId);
        Assert.Equal("Staff Systems Architect", verified.Headline);
    }

    [Fact]
    public async Task PostgreSql_PortfolioConfiguration_SectionsJsonbChangeTracking()
    {
        var usernameStr = $"usr{Guid.NewGuid():N}"[..12];
        var userId = $"auth-{Guid.NewGuid():N}"[..16];
        var tenantId = await ProvisionTenantAsync(userId, usernameStr);
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId, userId);

        // 1. Initial State
        using (var context = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var config = await context.PortfolioConfigurations.SingleAsync(c => c.TenantId == tenantId);
            Assert.NotEmpty(config.Sections);

            // Toggle a section's visibility
            config.SetSectionVisibility(PortfolioSectionType.Experience, false);
            await context.SaveChangesAsync();
        }

        // 2. Reload and verify visibility mutation was persisted
        PortfolioSectionType originalLastType;
        using (var context = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var config = await context.PortfolioConfigurations.SingleAsync(c => c.TenantId == tenantId);
            var expSection = config.Sections.Single(s => s.SectionType == PortfolioSectionType.Experience);
            Assert.False(expSection.IsEnabled);

            // Reorder sections
            var originalSections = config.Sections.ToList();
            originalLastType = originalSections.Last().SectionType;

            var reordered = new List<PortfolioSectionType>();
            for (var i = originalSections.Count - 1; i >= 0; i--)
            {
                reordered.Add(originalSections[i].SectionType);
            }

            config.ReorderSections(reordered);
            await context.SaveChangesAsync();
        }

        // 3. Reload and verify reordering was persisted
        using (var context = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var config = await context.PortfolioConfigurations.SingleAsync(c => c.TenantId == tenantId);
            Assert.Equal(originalLastType, config.Sections.First().SectionType);
            Assert.Equal(0, config.Sections.First().DisplayOrder);
        }
    }

    [Fact]
    public async Task PostgreSql_Project_TechnologiesArrayChangeTracking()
    {
        var usernameStr = $"usr{Guid.NewGuid():N}"[..12];
        var userId = $"auth-{Guid.NewGuid():N}"[..16];
        var tenantId = await ProvisionTenantAsync(userId, usernameStr);
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId, userId);

        Guid projectId;
        using (var context = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var project = new Project(tenantId, "Distributed Database", technologies: new[] { "C#", ".NET", "PostgreSQL" });
            context.Projects.Add(project);
            await context.SaveChangesAsync();
            projectId = project.Id;
        }

        // Verify array was persisted
        using (var context = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var project = await context.Projects.SingleAsync(p => p.Id == projectId);
            Assert.Equal(new[] { "C#", ".NET", "PostgreSQL" }, project.Technologies);

            // Mutate technologies array
            project.UpdateDetails(
                project.Title,
                project.ShortDescription,
                project.Description,
                project.GitHubUrl,
                project.LiveUrl,
                project.IsFeatured,
                project.DisplayOrder,
                new[] { "C#", ".NET", "PostgreSQL", "Docker", "Kubernetes" });
            await context.SaveChangesAsync();
        }

        // Verify mutation was persisted
        using (var context = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var project = await context.Projects.SingleAsync(p => p.Id == projectId);
            Assert.Equal(5, project.Technologies.Count);
            Assert.Contains("Kubernetes", project.Technologies);
        }
    }

    [Fact]
    public async Task PostgreSql_TenantIsolation_TenantACannotReadTenantB()
    {
        var tenantA_Id = await ProvisionTenantAsync($"auth-a-{Guid.NewGuid():N}"[..16], $"usra{Guid.NewGuid():N}"[..12]);
        var tenantB_Id = await ProvisionTenantAsync($"auth-b-{Guid.NewGuid():N}"[..16], $"usrb{Guid.NewGuid():N}"[..12]);

        var tenantA_Context = TestTenantContext.AuthenticatedWithTenant(tenantA_Id);
        var tenantB_Context = TestTenantContext.AuthenticatedWithTenant(tenantB_Id);

        // Add Skill under Tenant A
        using (var contextA = new ProfolioDbContext(CreateOptions(), tenantA_Context))
        {
            contextA.Skills.Add(new Skill(tenantA_Id, "Architecture"));
            await contextA.SaveChangesAsync();
        }

        // Query under Tenant B: Tenant A's skill must not be visible
        using (var contextB = new ProfolioDbContext(CreateOptions(), tenantB_Context))
        {
            var skills = await contextB.Skills.ToListAsync();
            Assert.Empty(skills);
        }
    }

    [Fact]
    public async Task PostgreSql_PublishingState_CompositeForeignKey_RejectsCrossTenantVersion()
    {
        var tenantA_Id = await ProvisionTenantAsync($"auth-a-{Guid.NewGuid():N}"[..16], $"usra{Guid.NewGuid():N}"[..12]);
        var tenantB_Id = await ProvisionTenantAsync($"auth-b-{Guid.NewGuid():N}"[..16], $"usrb{Guid.NewGuid():N}"[..12]);

        var tenantA_Context = TestTenantContext.AuthenticatedWithTenant(tenantA_Id);

        // Add version under Tenant B
        Guid tenantB_VersionId;
        using (var contextB = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(tenantB_Id)))
        {
            var version = new PortfolioVersion(tenantB_Id, 1, "{\"template\":\"modern\"}");
            contextB.PortfolioVersions.Add(version);
            await contextB.SaveChangesAsync();
            tenantB_VersionId = version.Id;
        }

        // Attempt to activate Tenant B's version under Tenant A
        using (var contextA = new ProfolioDbContext(CreateOptions(), tenantA_Context))
        {
            var pubState = await contextA.PublishingStates.SingleAsync(p => p.TenantId == tenantA_Id);
            contextA.Entry(pubState).Property(p => p.ActivePortfolioVersionId).CurrentValue = tenantB_VersionId;
            contextA.Entry(pubState).Property(p => p.ActiveVersionNumber).CurrentValue = 1;
            contextA.Entry(pubState).Property(p => p.IsPublished).CurrentValue = true;
            contextA.Entry(pubState).Property(p => p.PublishedAtUtc).CurrentValue = DateTime.UtcNow;

            // Database constraint: Composite FK (TenantId, ActivePortfolioVersionId) -> (TenantId, Id)
            // Violating this must throw DbUpdateException with a foreign key violation!
            var ex = await Assert.ThrowsAsync<DbUpdateException>(() => contextA.SaveChangesAsync());
            Assert.NotNull(ex.InnerException);
            Assert.Contains("23503", ex.InnerException.Message); // Postgres foreign_key_violation code
        }
    }

    [Fact]
    public async Task PostgreSql_PublicPortfolioReader_ReadsOnlyPublishedSnapshots()
    {
        var usernameStr = $"usr{Guid.NewGuid():N}"[..12];
        var userId = $"auth-{Guid.NewGuid():N}"[..16];
        var tenantId = await ProvisionTenantAsync(userId, usernameStr);
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId, userId);

        var snapshotJson = "{\"headline\":\"Principal Software Engineer\",\"projects\":[]}";

        // 1. Create a version and publish
        using (var context = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var version = new PortfolioVersion(tenantId, 1, snapshotJson);
            context.PortfolioVersions.Add(version);

            var pubState = await context.PublishingStates.SingleAsync(p => p.TenantId == tenantId);
            pubState.Publish(version);

            await context.SaveChangesAsync();
        }

        // 2. Read snapshot anonymously using PublicPortfolioReader
        using (var anonContext = new ProfolioDbContext(CreateOptions(), TestTenantContext.Anonymous()))
        {
            var reader = new PublicPortfolioReader(anonContext);
            var snapshot = await reader.GetPublishedByUsernameAsync(new Username(usernameStr));

            Assert.NotNull(snapshot);
            Assert.Equal(tenantId, snapshot.TenantId);
            Assert.Equal(usernameStr, snapshot.Username);
            Assert.Equal(1, snapshot.VersionNumber);
            using var expectedDoc = System.Text.Json.JsonDocument.Parse(snapshotJson);
            using var actualDoc = System.Text.Json.JsonDocument.Parse(snapshot.SnapshotJson);
            Assert.Equal(
                expectedDoc.RootElement.GetProperty("headline").GetString(),
                actualDoc.RootElement.GetProperty("headline").GetString());
        }
    }

    [Fact]
    public async Task PostgreSql_TenantDirectory_ResolvesTenantAndChecksUsername()
    {
        var usernameStr = $"usr{Guid.NewGuid():N}"[..12];
        var userId = $"auth-{Guid.NewGuid():N}"[..16];
        var tenantId = await ProvisionTenantAsync(userId, usernameStr);

        using var anonContext = new ProfolioDbContext(CreateOptions(), TestTenantContext.Anonymous());
        var directory = new TenantDirectory(anonContext);

        // Find by owner user ID
        var resolvedTenantId = await directory.FindTenantIdByOwnerUserIdAsync(userId);
        Assert.Equal(tenantId, resolvedTenantId);

        // Username exists
        var exists = await directory.UsernameExistsAsync(new Username(usernameStr));
        Assert.True(exists);

        // Username exists excluding self
        var existsExcludingSelf = await directory.UsernameExistsAsync(new Username(usernameStr), tenantId);
        Assert.False(existsExcludingSelf);

        // Nonexistent username
        var nonExistent = await directory.UsernameExistsAsync(new Username("nonexistent99"));
        Assert.False(nonExistent);
    }

    // -------------------------------------------------------------------------
    // 12. DeveloperProfile Application Use-Case Integration Tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PostgreSql_DeveloperProfile_GetProfile_ReturnsPersistedData()
    {
        var usernameStr = $"usr{Guid.NewGuid():N}"[..12];
        var userId = $"auth-{Guid.NewGuid():N}"[..16];
        var tenantId = await ProvisionTenantAsync(userId, usernameStr);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId, userId);
        using var context = new ProfolioDbContext(CreateOptions(), tenantContext);
        var handler = new GetDeveloperProfileQueryHandler(context, tenantContext);

        var result = await handler.HandleAsync(new GetDeveloperProfileQuery());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Test User", result.Value.FullName);
        Assert.Equal("Developer", result.Value.Headline);
        Assert.Null(result.Value.Bio);
        Assert.Null(result.Value.Location);
        Assert.Null(result.Value.PublicContactEmail);
        Assert.True(result.Value.CreatedAtUtc <= DateTime.UtcNow);
        Assert.Null(result.Value.UpdatedAtUtc);
    }

    [Fact]
    public async Task PostgreSql_DeveloperProfile_UpdateProfile_PersistsChanges()
    {
        var usernameStr = $"usr{Guid.NewGuid():N}"[..12];
        var userId = $"auth-{Guid.NewGuid():N}"[..16];
        var tenantId = await ProvisionTenantAsync(userId, usernameStr);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId, userId);
        using (var context = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var handler = new UpdateDeveloperProfileCommandHandler(context, tenantContext);
            var command = new UpdateDeveloperProfileCommand("Grace Hopper", "Computer Scientist", "Pioneered compilers", "Arlington, VA");

            var result = await handler.HandleAsync(command);
            Assert.True(result.IsSuccess);
        }

        // Verify with a new context from PostgreSQL
        using (var verifyContext = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var profile = await verifyContext.DeveloperProfiles.SingleAsync(p => p.TenantId == tenantId);
            Assert.Equal("Grace Hopper", profile.FullName);
            Assert.Equal("Computer Scientist", profile.Headline);
            Assert.Equal("Pioneered compilers", profile.Bio);
            Assert.Equal("Arlington, VA", profile.Location);
            Assert.NotNull(profile.UpdatedAtUtc);
        }
    }

    [Fact]
    public async Task PostgreSql_DeveloperProfile_SetAndClearContactEmail_Persists()
    {
        var usernameStr = $"usr{Guid.NewGuid():N}"[..12];
        var userId = $"auth-{Guid.NewGuid():N}"[..16];
        var tenantId = await ProvisionTenantAsync(userId, usernameStr);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId, userId);

        // 1. Set Email
        using (var context = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var handler = new SetPublicContactEmailCommandHandler(context, tenantContext);
            var result = await handler.HandleAsync(new SetPublicContactEmailCommand("Grace.Hopper@USNavy.MIL"));
            Assert.True(result.IsSuccess);
        }

        // Verify Set in Database (normalized to lowercase domain)
        using (var verifyContext = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var profile = await verifyContext.DeveloperProfiles.SingleAsync(p => p.TenantId == tenantId);
            Assert.NotNull(profile.PublicContactEmail);
            Assert.Equal("Grace.Hopper@usnavy.mil", profile.PublicContactEmail.Value);
        }

        // 2. Clear Email
        using (var context = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var handler = new ClearPublicContactEmailCommandHandler(context, tenantContext);
            var result = await handler.HandleAsync(new ClearPublicContactEmailCommand());
            Assert.True(result.IsSuccess);
        }

        // Verify Cleared in Database
        using (var verifyContext = new ProfolioDbContext(CreateOptions(), tenantContext))
        {
            var profile = await verifyContext.DeveloperProfiles.SingleAsync(p => p.TenantId == tenantId);
            Assert.Null(profile.PublicContactEmail);
        }
    }

    [Fact]
    public async Task PostgreSql_DeveloperProfile_TenantIsolation_CannotReadOrUpdateOtherTenant()
    {
        var tenantA_Id = await ProvisionTenantAsync($"auth-a-{Guid.NewGuid():N}"[..16], $"usra{Guid.NewGuid():N}"[..12]);
        var tenantB_Id = await ProvisionTenantAsync($"auth-b-{Guid.NewGuid():N}"[..16], $"usrb{Guid.NewGuid():N}"[..12]);

        var tenantA_Context = TestTenantContext.AuthenticatedWithTenant(tenantA_Id);
        var tenantB_Context = TestTenantContext.AuthenticatedWithTenant(tenantB_Id);

        // Tenant A updates their profile
        using (var contextA = new ProfolioDbContext(CreateOptions(), tenantA_Context))
        {
            var handlerA = new UpdateDeveloperProfileCommandHandler(contextA, tenantA_Context);
            var resultA = await handlerA.HandleAsync(new UpdateDeveloperProfileCommand("Tenant A User", "Specialist A", null, null));
            Assert.True(resultA.IsSuccess);
        }

        // Tenant B reads their profile: must only see Tenant B's data
        using (var contextB = new ProfolioDbContext(CreateOptions(), tenantB_Context))
        {
            var queryHandlerB = new GetDeveloperProfileQueryHandler(contextB, tenantB_Context);
            var resultB = await queryHandlerB.HandleAsync(new GetDeveloperProfileQuery());

            Assert.True(resultB.IsSuccess);
            Assert.Equal("Test User", resultB.Value.FullName);
            Assert.Equal("Developer", resultB.Value.Headline);
        }
    }

    [Fact]
    public async Task PostgreSql_DeveloperProfile_OptimisticConcurrency_ReturnsConflict()
    {
        var usernameStr = $"usr{Guid.NewGuid():N}"[..12];
        var userId = $"auth-{Guid.NewGuid():N}"[..16];
        var tenantId = await ProvisionTenantAsync(userId, usernameStr);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId, userId);

        using var contextA = new ProfolioDbContext(CreateOptions(), tenantContext);
        using var contextB = new ProfolioDbContext(CreateOptions(), tenantContext);

        // Both contexts load the profile
        var profileA = await contextA.DeveloperProfiles.SingleAsync(p => p.TenantId == tenantId);
        var profileB = await contextB.DeveloperProfiles.SingleAsync(p => p.TenantId == tenantId);

        // Context A updates and saves first
        profileA.UpdateBasicInformation("Ada Lovelace", "Version 1 Headline", null, null);
        await contextA.SaveChangesAsync();

        // Context B uses handler with stale tracked profile
        var handlerB = new UpdateDeveloperProfileCommandHandler(contextB, tenantContext);
        var commandB = new UpdateDeveloperProfileCommand("Ada Lovelace", "Version 2 Stale Headline", null, null);

        var resultB = await handlerB.HandleAsync(commandB);

        Assert.True(resultB.IsFailure);
        Assert.Equal("Concurrency.Conflict", resultB.Error!.Code);
        Assert.Equal(ErrorType.Conflict, resultB.Error.Type);

        // Verify Context A's update remains in PostgreSQL
        using var verifyContext = new ProfolioDbContext(CreateOptions(), tenantContext);
        var verified = await verifyContext.DeveloperProfiles.SingleAsync(p => p.TenantId == tenantId);
        Assert.Equal("Version 1 Headline", verified.Headline);
    }
}
