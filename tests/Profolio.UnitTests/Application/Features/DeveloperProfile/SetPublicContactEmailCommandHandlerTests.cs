namespace Profolio.UnitTests.Application.Features.DeveloperProfile;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Profolio.Application.Common.Results;
using Profolio.Application.Features.DeveloperProfile.Commands.SetPublicContactEmail;
using Profolio.Domain.Entities;
using Profolio.UnitTests.Application.TestDoubles;
using Profolio.UnitTests.Infrastructure.Persistence;
using Xunit;

public class SetPublicContactEmailCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenCommandIsNull_ThrowsArgumentNullException()
    {
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(Guid.NewGuid());
        var handler = new SetPublicContactEmailCommandHandler(dbContext, tenantContext);

        await Assert.ThrowsAsync<ArgumentNullException>("command", () => handler.HandleAsync(null!));
    }

    [Fact]
    public async Task HandleAsync_WhenUnauthenticated_ReturnsUnauthorizedError()
    {
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.Anonymous();
        var handler = new SetPublicContactEmailCommandHandler(dbContext, tenantContext);

        var command = new SetPublicContactEmailCommand("ada@analyticalengine.com");
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
        var handler = new SetPublicContactEmailCommandHandler(dbContext, tenantContext);

        var command = new SetPublicContactEmailCommand("ada@analyticalengine.com");
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("Tenant.NotResolved", result.Error!.Code);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("plainaddress")]
    [InlineData("missing@domain")]
    [InlineData("@missinglocal.com")]
    [InlineData("two@@domain.com")]
    [InlineData("has space@domain.com")]
    [InlineData("user@-badlabel.com")]
    [InlineData("user@badlabel-.com")]
    public async Task HandleAsync_WhenEmailInvalid_ReturnsInvalidEmailError(string? invalidEmail)
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new SetPublicContactEmailCommandHandler(dbContext, tenantContext);

        var command = new SetPublicContactEmailCommand(invalidEmail!);
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("DeveloperProfile.Email.Invalid", result.Error!.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Email", result.Error.Field);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailExceedsMaxLength_ReturnsInvalidEmailError()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new SetPublicContactEmailCommandHandler(dbContext, tenantContext);

        var excessiveEmail = new string('a', 250) + "@domain.com";
        var command = new SetPublicContactEmailCommand(excessiveEmail);
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("DeveloperProfile.Email.Invalid", result.Error!.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Email", result.Error.Field);
    }

    [Fact]
    public async Task HandleAsync_WhenProfileNotFound_ReturnsNotFoundError()
    {
        var tenantId = Guid.NewGuid();
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new SetPublicContactEmailCommandHandler(dbContext, tenantContext);

        var command = new SetPublicContactEmailCommand("ada@analyticalengine.com");
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("DeveloperProfile.NotFound", result.Error!.Code);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsync_WhenValidEmail_SetsEmailWithNormalizedDomainAndSaves()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new SetPublicContactEmailCommandHandler(dbContext, tenantContext);

        var command = new SetPublicContactEmailCommand("  Ada.Lovelace@AnalyticalEngine.ORG  ");
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(profile.PublicContactEmail);
        Assert.Equal("Ada.Lovelace@analyticalengine.org", profile.PublicContactEmail.Value);
        Assert.NotNull(profile.UpdatedAtUtc);
        Assert.Equal(1, dbContext.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_WhenSameEmailReapplied_PreservesState()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        profile.SetPublicContactEmail(new Profolio.Domain.ValueObjects.EmailAddress("ada@analyticalengine.org"));

        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new SetPublicContactEmailCommandHandler(dbContext, tenantContext);

        var command = new SetPublicContactEmailCommand("ada@analyticalengine.org");
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, dbContext.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_WhenDbUpdateConcurrencyExceptionOccurs_ReturnsConflictError()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var dbContext = new TestApplicationDbContext
        {
            SaveChangesBehavior = _ => throw new DbUpdateConcurrencyException("Stale row version.")
        };
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new SetPublicContactEmailCommandHandler(dbContext, tenantContext);

        var command = new SetPublicContactEmailCommand("ada@analyticalengine.com");
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
        var handler = new SetPublicContactEmailCommandHandler(dbContext, tenantContext);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new SetPublicContactEmailCommand("ada@analyticalengine.com");

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.HandleAsync(command, cts.Token));
    }
}
