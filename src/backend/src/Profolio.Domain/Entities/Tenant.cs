using Profolio.Domain.Common;
using Profolio.Domain.ValueObjects;

namespace Profolio.Domain.Entities;

public sealed class Tenant : BaseEntity, ITenantEntity
{
    public string OwnerUserId { get; private set; } = string.Empty;

    public Username Username { get; private set; } = null!;

    Guid ITenantEntity.TenantId => Id;

    private Tenant()
    {
        // Required by EF Core for entity materialization.
    }

    public Tenant(string ownerUserId, Username username)
    {
        ArgumentNullException.ThrowIfNull(ownerUserId);

        if (string.IsNullOrWhiteSpace(ownerUserId))
        {
            throw new ArgumentException("Owner user ID cannot be empty or whitespace.", nameof(ownerUserId));
        }

        ArgumentNullException.ThrowIfNull(username);

        OwnerUserId = ownerUserId;
        Username = username;
    }

    public void UpdateUsername(Username newUsername)
    {
        ArgumentNullException.ThrowIfNull(newUsername);

        if (Username == newUsername)
        {
            return;
        }

        Username = newUsername;
        MarkAsUpdated();
    }
}
