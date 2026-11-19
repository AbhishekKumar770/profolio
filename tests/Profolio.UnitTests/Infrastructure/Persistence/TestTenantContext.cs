namespace Profolio.UnitTests.Infrastructure.Persistence;

using System;
using Profolio.Application.Abstractions.Tenancy;

public sealed class TestTenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
    public string? UserId { get; set; }
    public bool IsAuthenticated { get; set; }

    public static TestTenantContext Anonymous() => new()
    {
        IsAuthenticated = false,
        UserId = null,
        TenantId = null
    };

    public static TestTenantContext AuthenticatedNoTenant(string userId = "user-123") => new()
    {
        IsAuthenticated = true,
        UserId = userId,
        TenantId = null
    };

    public static TestTenantContext AuthenticatedWithTenant(Guid tenantId, string userId = "user-123") => new()
    {
        IsAuthenticated = true,
        UserId = userId,
        TenantId = tenantId
    };
}
