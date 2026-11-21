namespace Profolio.Application.Features.Skills.Commands.UpdateSkill;

using Microsoft.EntityFrameworkCore;
using Profolio.Application.Abstractions.Persistence;
using Profolio.Application.Abstractions.Tenancy;
using Profolio.Application.Common.Handlers;
using Profolio.Application.Common.Results;
using Profolio.Application.Common.Validation;
using Profolio.Application.Features.Skills;
using Profolio.Application.Features.Skills.Commands.AddSkill;
using Profolio.Domain.Entities;

public sealed class UpdateSkillCommandHandler : ICommandHandler<UpdateSkillCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public UpdateSkillCommandHandler(IApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Result> HandleAsync(UpdateSkillCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (!_tenantContext.IsAuthenticated) return Result.Failure(ApplicationErrors.Tenancy.Unauthenticated());
        if (!_tenantContext.HasTenant()) return Result.Failure(ApplicationErrors.Tenancy.NotResolved());
        if (command.SkillId == Guid.Empty) return Result.Failure(SkillErrors.NotFound());
        if (string.IsNullOrWhiteSpace(command.Name)) return Result.Failure(SkillErrors.InvalidName("Skill name is required."));
        if (command.Name.Trim().Length > Skill.MaxNameLength) return Result.Failure(SkillErrors.InvalidName($"Skill name cannot exceed {Skill.MaxNameLength} characters."));
        if (command.Category?.Trim().Length > Skill.MaxCategoryLength) return Result.Failure(SkillErrors.InvalidCategory($"Skill category cannot exceed {Skill.MaxCategoryLength} characters."));
        if (command.DisplayOrder < 0) return Result.Failure(SkillErrors.InvalidDisplayOrder());

        var tenantId = _tenantContext.GetRequiredTenantId();
        var skill = await _dbContext.Skills.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == command.SkillId, cancellationToken);
        if (skill is null) return Result.Failure(SkillErrors.NotFound());
        var name = command.Name.Trim();
        var category = string.IsNullOrWhiteSpace(command.Category) ? null : command.Category.Trim();
        if (await _dbContext.Skills.AnyAsync(s => s.TenantId == tenantId && s.Id != command.SkillId && s.Name.ToLower() == name.ToLower(), cancellationToken))
            return Result.Failure(SkillErrors.DuplicateName());

        if (string.Equals(skill.Name, name, StringComparison.Ordinal) &&
            string.Equals(skill.Category, category, StringComparison.Ordinal) &&
            skill.DisplayOrder == command.DisplayOrder)
            return Result.Success();

        try
        {
            skill.UpdateDetails(name, category, command.DisplayOrder);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (ArgumentException ex) when (ex.ParamName == "name")
        {
            return Result.Failure(SkillErrors.InvalidName(ex.Message));
        }
        catch (ArgumentException ex) when (ex.ParamName == "category")
        {
            return Result.Failure(SkillErrors.InvalidCategory(ex.Message));
        }
        catch (ArgumentOutOfRangeException)
        {
            return Result.Failure(SkillErrors.InvalidDisplayOrder());
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(ApplicationErrors.Concurrency.Conflict());
        }
        catch (DbUpdateException ex) when (SkillDatabaseErrorMapper.IsDuplicateSkillNameViolation(ex))
        {
            return Result.Failure(SkillErrors.DuplicateName());
        }
    }
}
