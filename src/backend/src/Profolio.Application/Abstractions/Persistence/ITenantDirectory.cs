using Profolio.Domain.ValueObjects;

namespace Profolio.Application.Abstractions.Persistence;

public interface ITenantDirectory
{
    Task<Guid?> FindTenantIdByOwnerUserIdAsync(
        string ownerUserId,
        CancellationToken cancellationToken = default);

    Task<bool> UsernameExistsAsync(
        Username username,
        Guid? excludeTenantId = null,
        CancellationToken cancellationToken = default);
}
