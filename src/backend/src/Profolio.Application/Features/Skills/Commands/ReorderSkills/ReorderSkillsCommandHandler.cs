namespace Profolio.Application.Features.Skills.Commands.ReorderSkills;

using Microsoft.EntityFrameworkCore;
using Profolio.Application.Abstractions.Persistence;
using Profolio.Application.Abstractions.Tenancy;
using Profolio.Application.Common.Handlers;
using Profolio.Application.Common.Results;
using Profolio.Application.Common.Validation;
using Profolio.Application.Features.Skills;

public sealed class ReorderSkillsCommandHandler : ICommandHandler<ReorderSkillsCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ReorderSkillsCommandHandler(IApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Result> HandleAsync(ReorderSkillsCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (!_tenantContext.IsAuthenticated) return Result.Failure(ApplicationErrors.Tenancy.Unauthenticated());
        if (!_tenantContext.HasTenant()) return Result.Failure(ApplicationErrors.Tenancy.NotResolved());
        if (command.OrderedSkillIds is null) return Result.Failure(SkillErrors.InvalidReorder("An ordered skill ID list is required."));
        if (command.OrderedSkillIds.Any(id => id == Guid.Empty) || command.OrderedSkillIds.Distinct().Count() != command.OrderedSkillIds.Count)
            return Result.Failure(SkillErrors.InvalidReorder("Skill IDs must be non-empty and unique."));

        var tenantId = _tenantContext.GetRequiredTenantId();
        var skills = await _dbContext.Skills.Where(s => s.TenantId == tenantId).ToListAsync(cancellationToken);
        if (skills.Count != command.OrderedSkillIds.Count || !skills.Select(s => s.Id).ToHashSet().SetEquals(command.OrderedSkillIds))
            return Result.Failure(SkillErrors.InvalidReorder("The request must contain every skill belonging to the active tenant exactly once."));

        var byId = skills.ToDictionary(s => s.Id);
        var hasChanges = false;
        for (var index = 0; index < command.OrderedSkillIds.Count; index++)
        {
            var skill = byId[command.OrderedSkillIds[index]];
            if (skill.DisplayOrder != index)
            {
                skill.UpdateDetails(skill.Name, skill.Category, index);
                hasChanges = true;
            }
        }

        if (!hasChanges) return Result.Success();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(ApplicationErrors.Concurrency.Conflict());
        }
    }
}
