namespace Profolio.Application.Abstractions.Tenancy;

public static class TenantContextExtensions
{
    public static bool HasTenant(this ITenantContext tenantContext)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);

        return tenantContext.IsAuthenticated &&
               !string.IsNullOrWhiteSpace(tenantContext.UserId) &&
               tenantContext.TenantId is Guid tenantId &&
               tenantId != Guid.Empty;
    }

    public static Guid GetRequiredTenantId(this ITenantContext tenantContext)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);

        if (!tenantContext.IsAuthenticated)
        {
            throw new InvalidOperationException("Tenant context requires an authenticated user.");
        }

        if (string.IsNullOrWhiteSpace(tenantContext.UserId))
        {
            throw new InvalidOperationException("Authenticated user ID is missing from tenant context.");
        }

        if (!tenantContext.TenantId.HasValue || tenantContext.TenantId.Value == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant ID is not resolved in the current context.");
        }

        return tenantContext.TenantId.Value;
    }

    public static string GetRequiredUserId(this ITenantContext tenantContext)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);

        if (!tenantContext.IsAuthenticated)
        {
            throw new InvalidOperationException("Tenant context requires an authenticated user.");
        }

        if (string.IsNullOrWhiteSpace(tenantContext.UserId))
        {
            throw new InvalidOperationException("Authenticated user ID is missing from tenant context.");
        }

        return tenantContext.UserId;
    }
}
