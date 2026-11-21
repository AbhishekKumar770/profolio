namespace Profolio.Application.Features.Skills.Queries.GetSkills;

using Microsoft.EntityFrameworkCore;
using Profolio.Application.Abstractions.Persistence;
using Profolio.Application.Abstractions.Tenancy;
using Profolio.Application.Common.Handlers;
using Profolio.Application.Common.Results;
using Profolio.Application.Common.Validation;
using Profolio.Application.Features.Skills;

public sealed class GetSkillsQueryHandler : IQueryHandler<GetSkillsQuery, IReadOnlyList<SkillDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetSkillsQueryHandler(IApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Result<IReadOnlyList<SkillDto>>> HandleAsync(GetSkillsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (!_tenantContext.IsAuthenticated) return Result.Failure<IReadOnlyList<SkillDto>>(ApplicationErrors.Tenancy.Unauthenticated());
        if (!_tenantContext.HasTenant()) return Result.Failure<IReadOnlyList<SkillDto>>(ApplicationErrors.Tenancy.NotResolved());

        var tenantId = _tenantContext.GetRequiredTenantId();
        var skills = await _dbContext.Skills.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.DisplayOrder).ThenBy(s => s.CreatedAtUtc).ThenBy(s => s.Id)
            .Select(s => new SkillDto(s.Id, s.Name, s.Category, s.DisplayOrder, s.CreatedAtUtc, s.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<SkillDto>>(skills);
    }
}
