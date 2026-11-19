namespace Profolio.Infrastructure.Persistence.Services;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Profolio.Application.Abstractions.Persistence;
using Profolio.Domain.ValueObjects;

public sealed class PublicPortfolioReader : IPublicPortfolioReader
{
    private readonly ProfolioDbContext _context;

    public PublicPortfolioReader(ProfolioDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PublicPortfolioSnapshot?> GetPublishedByUsernameAsync(
        Username username,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(username);

        var query = from tenant in _context.Tenants.IgnoreQueryFilters(["TenantFilter"])
                    where tenant.Username == username
                    join pubState in _context.PublishingStates.IgnoreQueryFilters(["TenantFilter"])
                        on tenant.Id equals pubState.TenantId
                    where pubState.IsPublished && pubState.ActivePortfolioVersionId != null && pubState.PublishedAtUtc != null
                    join version in _context.PortfolioVersions.IgnoreQueryFilters(["TenantFilter"])
                        on new { TenantId = pubState.TenantId, VersionId = pubState.ActivePortfolioVersionId!.Value }
                        equals new { version.TenantId, VersionId = version.Id }
                    select new PublicPortfolioSnapshot(
                        tenant.Id,
                        tenant.Username.Value,
                        version.VersionNumber,
                        version.SnapshotJson,
                        version.SnapshotSchemaVersion,
                        pubState.PublishedAtUtc!.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
