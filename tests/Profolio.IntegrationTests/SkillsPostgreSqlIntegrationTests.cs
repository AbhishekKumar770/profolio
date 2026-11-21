namespace Profolio.IntegrationTests;

using Microsoft.EntityFrameworkCore;
using Npgsql;
using Profolio.Application.Common.Handlers;
using Profolio.Application.Features.Skills;
using Profolio.Application.Features.Skills.Commands.AddSkill;
using Profolio.Application.Features.Skills.Commands.DeleteSkill;
using Profolio.Application.Features.Skills.Commands.ReorderSkills;
using Profolio.Application.Features.Skills.Commands.UpdateSkill;
using Profolio.Application.Features.Skills.Queries.GetSkills;
using Profolio.Domain.Entities;
using Profolio.Infrastructure.Persistence;

public sealed partial class PostgreSqlIntegrationTests
{
    [Fact]
    public async Task PostgreSql_SkillsMigration_CreatesFunctionalTenantScopedUniqueIndex()
    {
        var tenantId = await ProvisionTenantAsync($"idx-{Guid.NewGuid():N}"[..16], $"idx{Guid.NewGuid():N}"[..12]);
        using var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(tenantId));

        var indexes = await context.Database.SqlQueryRaw<string>("""
            SELECT indexdef AS "Value"
            FROM pg_indexes
            WHERE schemaname = 'portfolio' AND indexname = 'ix_skills_tenant_id_lower_name'
            """).SingleAsync();

        Assert.Contains("UNIQUE INDEX", indexes, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ix_skills_tenant_id_lower_name", indexes, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"TenantId\"", indexes, StringComparison.Ordinal);
        Assert.Matches(@"(?i)\(\s*""TenantId""\s*,\s*lower\s*\(.*""Name"".*\)\s*\)", indexes);
    }

    [Fact]
    public async Task PostgreSql_SkillsUniqueIndex_RejectsCaseInsensitiveDuplicatesWithExactIndexName()
    {
        var tenantId = await ProvisionTenantAsync($"dup-{Guid.NewGuid():N}"[..16], $"dup{Guid.NewGuid():N}"[..12]);
        using (var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(tenantId)))
        {
            context.Skills.Add(new Skill(tenantId, "React"));
            await context.SaveChangesAsync();
        }

        using (var context = new ProfolioDbContext(CreateOptions(), TestTenantContext.AuthenticatedWithTenant(tenantId)))
        {
            context.Skills.Add(new Skill(tenantId, "react"));
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            var postgresException = Assert.IsType<PostgresException>(exception.InnerException);
            Assert.Equal(PostgresErrorCodes.UniqueViolation, postgresException.SqlState);
            Assert.Equal(SkillErrors.UniqueIndexName, postgresException.ConstraintName);
        }
    }

    [Fact]
    public async Task PostgreSql_SkillsHandlers_EnforceTenantIsolationAndCompleteAtomicReorder()
    {
        var tenantA = await ProvisionTenantAsync($"sk-a-{Guid.NewGuid():N}"[..16], $"ska{Guid.NewGuid():N}"[..12]);
        var tenantB = await ProvisionTenantAsync($"sk-b-{Guid.NewGuid():N}"[..16], $"skb{Guid.NewGuid():N}"[..12]);
        var tenantAContext = TestTenantContext.AuthenticatedWithTenant(tenantA);
        var tenantBContext = TestTenantContext.AuthenticatedWithTenant(tenantB);
        Guid firstId;
        Guid secondId;

        using (var context = new ProfolioDbContext(CreateOptions(), tenantAContext))
        {
            context.Skills.AddRange(new Skill(tenantA, "First", displayOrder: 0), new Skill(tenantA, "Second", displayOrder: 1));
            await context.SaveChangesAsync();
            firstId = await context.Skills.Where(s => s.Name == "First").Select(s => s.Id).SingleAsync();
            secondId = await context.Skills.Where(s => s.Name == "Second").Select(s => s.Id).SingleAsync();
        }

        using (var context = new ProfolioDbContext(CreateOptions(), tenantBContext))
        {
            var update = await new UpdateSkillCommandHandler(context, tenantBContext)
                .HandleAsync(new UpdateSkillCommand(firstId, "Tampered", null, 0));
            Assert.Equal("Skill.NotFound", update.Error!.Code);
        }

        using (var context = new ProfolioDbContext(CreateOptions(), tenantAContext))
        {
            var partial = await new ReorderSkillsCommandHandler(context, tenantAContext)
                .HandleAsync(new ReorderSkillsCommand([secondId]));
            Assert.Equal("Skill.Reorder.Invalid", partial.Error!.Code);

            var reordered = await new ReorderSkillsCommandHandler(context, tenantAContext)
                .HandleAsync(new ReorderSkillsCommand([secondId, firstId]));
            Assert.True(reordered.IsSuccess);
        }

        using (var context = new ProfolioDbContext(CreateOptions(), tenantAContext))
        {
            var skills = await new GetSkillsQueryHandler(context, tenantAContext).HandleAsync(new GetSkillsQuery());
            Assert.True(skills.IsSuccess);
            Assert.Equal(new[] { secondId, firstId }, skills.Value.Select(s => s.Id));
            Assert.Equal(new[] { 0, 1 }, skills.Value.Select(s => s.DisplayOrder));
        }
    }

    [Fact]
    public async Task PostgreSql_SkillsHandlers_AddDuplicateAndDelete()
    {
        var tenantId = await ProvisionTenantAsync($"cmd-{Guid.NewGuid():N}"[..16], $"cmd{Guid.NewGuid():N}"[..12]);
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        using var context = new ProfolioDbContext(CreateOptions(), tenantContext);
        var handler = new AddSkillCommandHandler(context, tenantContext);
        var added = await handler.HandleAsync(new AddSkillCommand("PostgreSQL", "Data"));
        Assert.True(added.IsSuccess);
        var duplicate = await handler.HandleAsync(new AddSkillCommand("postgresql", null));
        Assert.Equal("Skill.Name.Duplicate", duplicate.Error!.Code);

        var deleted = await new DeleteSkillCommandHandler(context, tenantContext).HandleAsync(new DeleteSkillCommand(added.Value));
        Assert.True(deleted.IsSuccess);
        Assert.Empty(await context.Skills.ToListAsync());
    }
}
