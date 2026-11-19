namespace Profolio.Infrastructure.Persistence.Services;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Profolio.Application.Abstractions.Persistence;
using Profolio.Domain.ValueObjects;

public sealed class TenantDirectory : ITenantDirectory
{
    private readonly ProfolioDbContext _context;

    public TenantDirectory(ProfolioDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Guid?> FindTenantIdByOwnerUserIdAsync(
        string ownerUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ownerUserId);

        if (string.IsNullOrWhiteSpace(ownerUserId))
        {
            return null;
        }

        return await _context.Tenants
            .IgnoreQueryFilters(["TenantFilter"])
            .Where(t => t.OwnerUserId == ownerUserId)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(
        Username username,
        Guid? excludeTenantId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(username);

        var query = _context.Tenants
            .IgnoreQueryFilters(["TenantFilter"])
            .Where(t => t.Username == username);

        if (excludeTenantId.HasValue && excludeTenantId.Value != Guid.Empty)
        {
            query = query.Where(t => t.Id != excludeTenantId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
