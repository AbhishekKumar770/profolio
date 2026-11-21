namespace Profolio.Application.Features.DeveloperProfile.Commands.UpdateDeveloperProfile;

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

public sealed class UpdateDeveloperProfileCommandHandler : ICommandHandler<UpdateDeveloperProfileCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public UpdateDeveloperProfileCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Result> HandleAsync(
        UpdateDeveloperProfileCommand command,
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

        try
        {
            profile.UpdateBasicInformation(
                command.FullName,
                command.Headline,
                command.Bio,
                command.Location);
        }
        catch (ArgumentException ex) when (ex.ParamName == "fullName")
        {
            return Result.Failure(
                string.IsNullOrWhiteSpace(command.FullName)
                    ? DeveloperProfileErrors.EmptyFullName()
                    : DeveloperProfileErrors.FullNameTooLong());
        }
        catch (ArgumentException ex) when (ex.ParamName == "headline")
        {
            return Result.Failure(
                string.IsNullOrWhiteSpace(command.Headline)
                    ? DeveloperProfileErrors.EmptyHeadline()
                    : DeveloperProfileErrors.HeadlineTooLong());
        }
        catch (ArgumentException ex) when (ex.ParamName == "location")
        {
            return Result.Failure(DeveloperProfileErrors.LocationTooLong());
        }

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
