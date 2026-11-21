namespace Profolio.UnitTests.Application.Features.DeveloperProfile;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Profolio.Application.Common.Results;
using Profolio.Application.Features.DeveloperProfile.Commands.ClearPublicContactEmail;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;
using Profolio.UnitTests.Application.TestDoubles;
using Profolio.UnitTests.Infrastructure.Persistence;
using Xunit;

public class ClearPublicContactEmailCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenCommandIsNull_ThrowsArgumentNullException()
    {
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(Guid.NewGuid());
        var handler = new ClearPublicContactEmailCommandHandler(dbContext, tenantContext);

        await Assert.ThrowsAsync<ArgumentNullException>("command", () => handler.HandleAsync(null!));
    }

    [Fact]
    public async Task HandleAsync_WhenUnauthenticated_ReturnsUnauthorizedError()
    {
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.Anonymous();
        var handler = new ClearPublicContactEmailCommandHandler(dbContext, tenantContext);

        var command = new ClearPublicContactEmailCommand();
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.Unauthenticated", result.Error!.Code);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsync_WhenTenantNotResolved_ReturnsForbiddenError()
    {
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.AuthenticatedNoTenant("user-1");
        var handler = new ClearPublicContactEmailCommandHandler(dbContext, tenantContext);

        var command = new ClearPublicContactEmailCommand();
        var result = await handler.HandleAsync(command);

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
        var handler = new ClearPublicContactEmailCommandHandler(dbContext, tenantContext);

        var command = new ClearPublicContactEmailCommand();
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("DeveloperProfile.NotFound", result.Error!.Code);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailExists_ClearsEmailAndSaves()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        profile.SetPublicContactEmail(new EmailAddress("ada@analyticalengine.com"));

        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new ClearPublicContactEmailCommandHandler(dbContext, tenantContext);

        var command = new ClearPublicContactEmailCommand();
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Null(profile.PublicContactEmail);
        Assert.NotNull(profile.UpdatedAtUtc);
        Assert.Equal(1, dbContext.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyNull_ReturnsSuccessNoOp()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");

        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new ClearPublicContactEmailCommandHandler(dbContext, tenantContext);

        var command = new ClearPublicContactEmailCommand();
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Null(profile.PublicContactEmail);
        Assert.Null(profile.UpdatedAtUtc);
        Assert.Equal(1, dbContext.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_WhenDbUpdateConcurrencyExceptionOccurs_ReturnsConflictError()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        profile.SetPublicContactEmail(new EmailAddress("ada@analyticalengine.com"));

        var dbContext = new TestApplicationDbContext
        {
            SaveChangesBehavior = _ => throw new DbUpdateConcurrencyException("Stale row version.")
        };
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new ClearPublicContactEmailCommandHandler(dbContext, tenantContext);

        var command = new ClearPublicContactEmailCommand();
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("Concurrency.Conflict", result.Error!.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        var tenantId = Guid.NewGuid();
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new ClearPublicContactEmailCommandHandler(dbContext, tenantContext);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new ClearPublicContactEmailCommand();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.HandleAsync(command, cts.Token));
    }
}
