namespace Profolio.Application.Abstractions.Tenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }

    string? UserId { get; }

    bool IsAuthenticated { get; }
}
