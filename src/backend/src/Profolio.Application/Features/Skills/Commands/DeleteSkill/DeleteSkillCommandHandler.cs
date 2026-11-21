namespace Profolio.Application.Features.Skills.Commands.DeleteSkill;

using Microsoft.EntityFrameworkCore;
using Profolio.Application.Abstractions.Persistence;
using Profolio.Application.Abstractions.Tenancy;
using Profolio.Application.Common.Handlers;
using Profolio.Application.Common.Results;
using Profolio.Application.Common.Validation;
using Profolio.Application.Features.Skills;

public sealed class DeleteSkillCommandHandler : ICommandHandler<DeleteSkillCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public DeleteSkillCommandHandler(IApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Result> HandleAsync(DeleteSkillCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (!_tenantContext.IsAuthenticated) return Result.Failure(ApplicationErrors.Tenancy.Unauthenticated());
        if (!_tenantContext.HasTenant()) return Result.Failure(ApplicationErrors.Tenancy.NotResolved());
        if (command.SkillId == Guid.Empty) return Result.Failure(SkillErrors.NotFound());
        var tenantId = _tenantContext.GetRequiredTenantId();
        var skill = await _dbContext.Skills.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == command.SkillId, cancellationToken);
        if (skill is null) return Result.Failure(SkillErrors.NotFound());
        _dbContext.Skills.Remove(skill);
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
