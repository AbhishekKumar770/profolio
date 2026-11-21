namespace Profolio.Application.Features.Skills.Commands.AddSkill;

using Microsoft.EntityFrameworkCore;
using Profolio.Application.Abstractions.Persistence;
using Profolio.Application.Abstractions.Tenancy;
using Profolio.Application.Common.Handlers;
using Profolio.Application.Common.Results;
using Profolio.Application.Common.Validation;
using Profolio.Application.Features.Skills;
using Profolio.Domain.Entities;

public sealed class AddSkillCommandHandler : ICommandHandler<AddSkillCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public AddSkillCommandHandler(IApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Result<Guid>> HandleAsync(AddSkillCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (!_tenantContext.IsAuthenticated) return Result.Failure<Guid>(ApplicationErrors.Tenancy.Unauthenticated());
        if (!_tenantContext.HasTenant()) return Result.Failure<Guid>(ApplicationErrors.Tenancy.NotResolved());
        if (string.IsNullOrWhiteSpace(command.Name)) return Result.Failure<Guid>(SkillErrors.InvalidName("Skill name is required."));
        if (command.Name.Trim().Length > Skill.MaxNameLength) return Result.Failure<Guid>(SkillErrors.InvalidName($"Skill name cannot exceed {Skill.MaxNameLength} characters."));
        if (command.Category?.Trim().Length > Skill.MaxCategoryLength) return Result.Failure<Guid>(SkillErrors.InvalidCategory($"Skill category cannot exceed {Skill.MaxCategoryLength} characters."));

        var tenantId = _tenantContext.GetRequiredTenantId();
        var tenantSkills = await _dbContext.Skills.Where(s => s.TenantId == tenantId).ToListAsync(cancellationToken);
        var name = command.Name.Trim();
        if (tenantSkills.Any(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<Guid>(SkillErrors.DuplicateName());

        var displayOrder = tenantSkills.Count == 0 ? 0 : checked(tenantSkills.Max(s => s.DisplayOrder) + 1);
        var skill = new Skill(tenantId, name, command.Category, displayOrder);
        _dbContext.Skills.Add(skill);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(skill.Id);
        }
        catch (DbUpdateException ex) when (SkillDatabaseErrorMapper.IsDuplicateSkillNameViolation(ex))
        {
            return Result.Failure<Guid>(SkillErrors.DuplicateName());
        }
    }

}
