namespace Profolio.UnitTests.Application.Features.DeveloperProfile;

using System;
using System.Threading;
using System.Threading.Tasks;
using Profolio.Application.Common.Results;
using Profolio.Application.Features.DeveloperProfile.Queries.GetDeveloperProfile;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;
using Profolio.UnitTests.Application.TestDoubles;
using Profolio.UnitTests.Infrastructure.Persistence;
using Xunit;

public class GetDeveloperProfileQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenQueryIsNull_ThrowsArgumentNullException()
    {
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(Guid.NewGuid());
        var handler = new GetDeveloperProfileQueryHandler(dbContext, tenantContext);

        await Assert.ThrowsAsync<ArgumentNullException>("query", () => handler.HandleAsync(null!));
    }

    [Fact]
    public async Task HandleAsync_WhenUnauthenticated_ReturnsUnauthorizedError()
    {
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.Anonymous();
        var handler = new GetDeveloperProfileQueryHandler(dbContext, tenantContext);

        var result = await handler.HandleAsync(new GetDeveloperProfileQuery());

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.Unauthenticated", result.Error!.Code);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsync_WhenTenantNotResolved_ReturnsForbiddenError()
    {
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.AuthenticatedNoTenant("user-1");
        var handler = new GetDeveloperProfileQueryHandler(dbContext, tenantContext);

        var result = await handler.HandleAsync(new GetDeveloperProfileQuery());

        Assert.True(result.IsFailure);
        Assert.Equal("Tenant.NotResolved", result.Error!.Code);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsync_WhenProfileNotFound_ReturnsNotFoundError()
    {
        var tenantId = Guid.NewGuid();
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new GetDeveloperProfileQueryHandler(dbContext, tenantContext);

        var result = await handler.HandleAsync(new GetDeveloperProfileQuery());

        Assert.True(result.IsFailure);
        Assert.Equal("DeveloperProfile.NotFound", result.Error!.Code);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsync_WhenProfileExists_ReturnsSuccessWithDto()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Pioneering Programmer");
        profile.UpdateBasicInformation("Ada Lovelace", "Pioneering Programmer", "Mathematician & writer", "London, UK");
        profile.SetPublicContactEmail(new EmailAddress("ada@analyticalengine.com"));

        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new GetDeveloperProfileQueryHandler(dbContext, tenantContext);

        var result = await handler.HandleAsync(new GetDeveloperProfileQuery());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(profile.Id, result.Value.Id);
        Assert.Equal("Ada Lovelace", result.Value.FullName);
        Assert.Equal("Pioneering Programmer", result.Value.Headline);
        Assert.Equal("Mathematician & writer", result.Value.Bio);
        Assert.Equal("London, UK", result.Value.Location);
        Assert.Equal("ada@analyticalengine.com", result.Value.PublicContactEmail);
        Assert.Equal(profile.CreatedAtUtc, result.Value.CreatedAtUtc);
        Assert.Equal(profile.UpdatedAtUtc, result.Value.UpdatedAtUtc);
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        var tenantId = Guid.NewGuid();
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new GetDeveloperProfileQueryHandler(dbContext, tenantContext);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.HandleAsync(new GetDeveloperProfileQuery(), cts.Token));
    }
}
