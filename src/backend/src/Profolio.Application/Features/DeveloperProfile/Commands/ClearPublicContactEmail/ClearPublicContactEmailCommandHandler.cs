namespace Profolio.Application.Features.DeveloperProfile.Commands.ClearPublicContactEmail;

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

public sealed class ClearPublicContactEmailCommandHandler : ICommandHandler<ClearPublicContactEmailCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ClearPublicContactEmailCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Result> HandleAsync(
        ClearPublicContactEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!_tenantContext.IsAuthenticated)
        {
            return Result.Failure(ApplicationErrors.Tenancy.Unauthenticated());
        }

        if (!_tenantContext.HasTenant())
        {
            return Result.Failure(ApplicationErrors.Tenancy.NotResolved());
        }

        var profile = await _dbContext.DeveloperProfiles
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return Result.Failure(DeveloperProfileErrors.NotFound());
        }

        profile.ClearPublicContactEmail();

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
