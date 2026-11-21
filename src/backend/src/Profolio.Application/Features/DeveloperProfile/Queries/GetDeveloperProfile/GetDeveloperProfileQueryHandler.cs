namespace Profolio.Application.Features.DeveloperProfile.Queries.GetDeveloperProfile;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Profolio.Application.Abstractions.Persistence;
using Profolio.Application.Abstractions.Tenancy;
using Profolio.Application.Common.Handlers;
using Profolio.Application.Common.Results;
using Profolio.Application.Common.Validation;
using Profolio.Application.Features.DeveloperProfile.Validation;

public sealed class GetDeveloperProfileQueryHandler : IQueryHandler<GetDeveloperProfileQuery, DeveloperProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetDeveloperProfileQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Result<DeveloperProfileDto>> HandleAsync(
        GetDeveloperProfileQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!_tenantContext.IsAuthenticated)
        {
            return Result.Failure<DeveloperProfileDto>(ApplicationErrors.Tenancy.Unauthenticated());
        }

        if (!_tenantContext.HasTenant())
        {
            return Result.Failure<DeveloperProfileDto>(ApplicationErrors.Tenancy.NotResolved());
        }

        var profile = await _dbContext.DeveloperProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return Result.Failure<DeveloperProfileDto>(DeveloperProfileErrors.NotFound());
        }

        var dto = new DeveloperProfileDto(
            profile.Id,
            profile.FullName,
            profile.Headline,
            profile.Bio,
            profile.Location,
            profile.PublicContactEmail?.Value,
            profile.CreatedAtUtc,
            profile.UpdatedAtUtc);

        return Result.Success(dto);
    }
}
