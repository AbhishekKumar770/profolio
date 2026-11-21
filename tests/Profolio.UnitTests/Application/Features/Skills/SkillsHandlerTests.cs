namespace Profolio.UnitTests.Application.Features.Skills;

using Profolio.Application.Common.Results;
using Profolio.Application.Features.Skills;
using Profolio.Application.Features.Skills.Commands.AddSkill;
using Profolio.Application.Features.Skills.Commands.DeleteSkill;
using Profolio.Application.Features.Skills.Commands.ReorderSkills;
using Profolio.Application.Features.Skills.Commands.UpdateSkill;
using Profolio.Application.Features.Skills.Queries.GetSkills;
using Profolio.Domain.Entities;
using Profolio.UnitTests.Application.TestDoubles;
using Profolio.UnitTests.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public sealed class SkillsHandlerTests
{
    [Fact]
    public async Task GetSkills_ReturnsDeterministicallyOrderedTenantSkills()
    {
        var tenantId = Guid.NewGuid();
        var context = new TestApplicationDbContext();
        var later = new Skill(tenantId, "Z", displayOrder: 1);
        var first = new Skill(tenantId, "B", displayOrder: 0);
        var second = new Skill(tenantId, "A", displayOrder: 0);
        context.Skills.Data.AddRange([later, first, second, new Skill(Guid.NewGuid(), "Other", displayOrder: 0)]);

        var result = await new GetSkillsQueryHandler(context, TestTenantContext.AuthenticatedWithTenant(tenantId))
            .HandleAsync(new GetSkillsQuery());

        Assert.True(result.IsSuccess);
        var expectedIds = new[] { first, second, later }
            .OrderBy(s => s.DisplayOrder).ThenBy(s => s.CreatedAtUtc).ThenBy(s => s.Id)
            .Select(s => s.Id);
        Assert.Equal(expectedIds, result.Value.Select(s => s.Id));
    }

    [Fact]
    public async Task AddSkill_TrimsNameAndAppendsAfterMaximumDisplayOrder()
    {
        var tenantId = Guid.NewGuid();
        var context = new TestApplicationDbContext();
        context.Skills.Data.Add(new Skill(tenantId, "Existing", displayOrder: 4));
        var result = await new AddSkillCommandHandler(context, TestTenantContext.AuthenticatedWithTenant(tenantId))
            .HandleAsync(new AddSkillCommand("  C#  ", "  Backend "));

        Assert.True(result.IsSuccess);
        var added = Assert.Single(context.Skills.Data, s => s.Id == result.Value);
        Assert.Equal("C#", added.Name);
        Assert.Equal("Backend", added.Category);
        Assert.Equal(5, added.DisplayOrder);
        Assert.Equal(1, context.SaveChangesCalls);
    }

    [Fact]
    public async Task AddSkill_RejectsCaseInsensitiveDuplicateWithoutSaving()
    {
        var tenantId = Guid.NewGuid();
        var context = new TestApplicationDbContext();
        context.Skills.Data.Add(new Skill(tenantId, "React"));
        var result = await new AddSkillCommandHandler(context, TestTenantContext.AuthenticatedWithTenant(tenantId))
            .HandleAsync(new AddSkillCommand("react", null));

        Assert.Equal("Skill.Name.Duplicate", result.Error!.Code);
        Assert.Equal(0, context.SaveChangesCalls);
    }

    [Fact]
    public void AddSkill_MapsOnlyTheExactPostgreSqlUniqueIndexViolation()
    {
        var expected = new DbUpdateException("failed", new PostgresException(
            "duplicate", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation,
            constraintName: SkillErrors.UniqueIndexName));
        var unrelated = new DbUpdateException("failed", new PostgresException(
            "duplicate", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation,
            constraintName: "some_other_unique_index"));
        var otherSqlState = new DbUpdateException("failed", new PostgresException(
            "failed", "ERROR", "ERROR", PostgresErrorCodes.CheckViolation,
            constraintName: SkillErrors.UniqueIndexName));

        Assert.True(SkillDatabaseErrorMapper.IsDuplicateSkillNameViolation(expected));
        Assert.False(SkillDatabaseErrorMapper.IsDuplicateSkillNameViolation(unrelated));
        Assert.False(SkillDatabaseErrorMapper.IsDuplicateSkillNameViolation(otherSqlState));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddSkill_RejectsBlankName(string name)
    {
        var result = await new AddSkillCommandHandler(new TestApplicationDbContext(), TestTenantContext.AuthenticatedWithTenant(Guid.NewGuid()))
            .HandleAsync(new AddSkillCommand(name, null));
        Assert.Equal("Skill.Name.Invalid", result.Error!.Code);
    }

    [Fact]
    public async Task AddSkill_PropagatesCancellationToPersistence()
    {
        var tenantId = Guid.NewGuid();
        var context = new TestApplicationDbContext();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new AddSkillCommandHandler(context, TestTenantContext.AuthenticatedWithTenant(tenantId))
                .HandleAsync(new AddSkillCommand("C#", null), cancellation.Token));
    }

    [Fact]
    public async Task UpdateSkill_RejectsForeignTenantSkill()
    {
        var context = new TestApplicationDbContext();
        var skill = new Skill(Guid.NewGuid(), "C#");
        context.Skills.Data.Add(skill);
        var result = await new UpdateSkillCommandHandler(context, TestTenantContext.AuthenticatedWithTenant(Guid.NewGuid()))
            .HandleAsync(new UpdateSkillCommand(skill.Id, "C#", null, 0));
        Assert.Equal("Skill.NotFound", result.Error!.Code);
        Assert.Equal(0, context.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateSkill_MapsCaseInsensitiveDuplicate()
    {
        var tenantId = Guid.NewGuid();
        var context = new TestApplicationDbContext();
        var skill = new Skill(tenantId, "C#");
        context.Skills.Data.AddRange([skill, new Skill(tenantId, "React")]);
        var result = await new UpdateSkillCommandHandler(context, TestTenantContext.AuthenticatedWithTenant(tenantId))
            .HandleAsync(new UpdateSkillCommand(skill.Id, "REACT", null, 0));
        Assert.Equal("Skill.Name.Duplicate", result.Error!.Code);
    }

    [Fact]
    public async Task UpdateSkill_NoOpDoesNotSave()
    {
        var tenantId = Guid.NewGuid();
        var context = new TestApplicationDbContext();
        var skill = new Skill(tenantId, "C#", "Backend", 2);
        context.Skills.Data.Add(skill);
        var result = await new UpdateSkillCommandHandler(context, TestTenantContext.AuthenticatedWithTenant(tenantId))
            .HandleAsync(new UpdateSkillCommand(skill.Id, "C#", "Backend", 2));
        Assert.True(result.IsSuccess);
        Assert.Equal(0, context.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateSkill_MapsConcurrencyConflict()
    {
        var tenantId = Guid.NewGuid();
        var context = new TestApplicationDbContext { SaveChangesBehavior = _ => throw new DbUpdateConcurrencyException() };
        var skill = new Skill(tenantId, "C#");
        context.Skills.Data.Add(skill);
        var result = await new UpdateSkillCommandHandler(context, TestTenantContext.AuthenticatedWithTenant(tenantId))
            .HandleAsync(new UpdateSkillCommand(skill.Id, "C Sharp", null, 0));
        Assert.Equal("Concurrency.Conflict", result.Error!.Code);
    }

    [Fact]
    public async Task DeleteSkill_RemovesOnlyActiveTenantRecord()
    {
        var tenantId = Guid.NewGuid();
        var context = new TestApplicationDbContext();
        var skill = new Skill(tenantId, "C#");
        context.Skills.Data.Add(skill);
        var result = await new DeleteSkillCommandHandler(context, TestTenantContext.AuthenticatedWithTenant(tenantId))
            .HandleAsync(new DeleteSkillCommand(skill.Id));
        Assert.True(result.IsSuccess);
        Assert.Empty(context.Skills.Data);
        Assert.Equal(1, context.SaveChangesCalls);
    }

    [Fact]
    public async Task ReorderSkills_RejectsIncompleteCollection()
    {
        var tenantId = Guid.NewGuid();
        var context = new TestApplicationDbContext();
        var first = new Skill(tenantId, "First", displayOrder: 0);
        var second = new Skill(tenantId, "Second", displayOrder: 1);
        context.Skills.Data.AddRange([first, second]);
        var result = await new ReorderSkillsCommandHandler(context, TestTenantContext.AuthenticatedWithTenant(tenantId))
            .HandleAsync(new ReorderSkillsCommand([first.Id]));
        Assert.Equal("Skill.Reorder.Invalid", result.Error!.Code);
        Assert.Equal(0, context.SaveChangesCalls);
    }

    [Fact]
    public async Task ReorderSkills_RejectsDuplicateIds()
    {
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "Only");
        var context = new TestApplicationDbContext();
        context.Skills.Data.Add(skill);
        var result = await new ReorderSkillsCommandHandler(context, TestTenantContext.AuthenticatedWithTenant(tenantId))
            .HandleAsync(new ReorderSkillsCommand([skill.Id, skill.Id]));
        Assert.Equal("Skill.Reorder.Invalid", result.Error!.Code);
    }

    [Fact]
    public async Task ReorderSkills_SetsContiguousPositionsAndSavesOnce()
    {
        var tenantId = Guid.NewGuid();
        var first = new Skill(tenantId, "First", displayOrder: 4);
        var second = new Skill(tenantId, "Second", displayOrder: 4);
        var context = new TestApplicationDbContext();
        context.Skills.Data.AddRange([first, second]);
        var result = await new ReorderSkillsCommandHandler(context, TestTenantContext.AuthenticatedWithTenant(tenantId))
            .HandleAsync(new ReorderSkillsCommand([second.Id, first.Id]));
        Assert.True(result.IsSuccess);
        Assert.Equal(0, second.DisplayOrder);
        Assert.Equal(1, first.DisplayOrder);
        Assert.Equal(1, context.SaveChangesCalls);
    }

    [Fact]
    public async Task ReorderSkills_EmptyTenantAndEmptyListSucceedsWithoutSave()
    {
        var context = new TestApplicationDbContext();
        var result = await new ReorderSkillsCommandHandler(context, TestTenantContext.AuthenticatedWithTenant(Guid.NewGuid()))
            .HandleAsync(new ReorderSkillsCommand([]));
        Assert.True(result.IsSuccess);
        Assert.Equal(0, context.SaveChangesCalls);
    }

    [Fact]
    public async Task ReorderSkills_MapsConcurrencyConflict()
    {
        var tenantId = Guid.NewGuid();
        var skill = new Skill(tenantId, "Only", displayOrder: 1);
        var context = new TestApplicationDbContext { SaveChangesBehavior = _ => throw new DbUpdateConcurrencyException() };
        context.Skills.Data.Add(skill);
        var result = await new ReorderSkillsCommandHandler(context, TestTenantContext.AuthenticatedWithTenant(tenantId))
            .HandleAsync(new ReorderSkillsCommand([skill.Id]));
        Assert.Equal("Concurrency.Conflict", result.Error!.Code);
    }

    [Fact]
    public async Task Handlers_RejectUnauthenticatedRequests()
    {
        var result = await new GetSkillsQueryHandler(new TestApplicationDbContext(), TestTenantContext.Anonymous())
            .HandleAsync(new GetSkillsQuery());
        Assert.Equal("Auth.Unauthenticated", result.Error!.Code);
    }
}
