namespace Profolio.UnitTests.Application.Features.DeveloperProfile;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Profolio.Application.Common.Results;
using Profolio.Application.Features.DeveloperProfile.Commands.UpdateDeveloperProfile;
using Profolio.Domain.Entities;
using Profolio.UnitTests.Application.TestDoubles;
using Profolio.UnitTests.Infrastructure.Persistence;
using Xunit;

public class UpdateDeveloperProfileCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenCommandIsNull_ThrowsArgumentNullException()
    {
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.AuthenticatedWithTenant(Guid.NewGuid());
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        await Assert.ThrowsAsync<ArgumentNullException>("command", () => handler.HandleAsync(null!));
    }

    [Fact]
    public async Task HandleAsync_WhenUnauthenticated_ReturnsUnauthorizedError()
    {
        var dbContext = new TestApplicationDbContext();
        var tenantContext = TestTenantContext.Anonymous();
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        var command = new UpdateDeveloperProfileCommand("Ada Lovelace", "Programmer", null, null);
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
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        var command = new UpdateDeveloperProfileCommand("Ada Lovelace", "Programmer", null, null);
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
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        var command = new UpdateDeveloperProfileCommand("Ada Lovelace", "Programmer", null, null);
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("DeveloperProfile.NotFound", result.Error!.Code);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HandleAsync_WhenFullNameEmptyOrWhitespace_ReturnsEmptyFullNameError(string? invalidFullName)
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Original Name", "Original Headline");
        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        var command = new UpdateDeveloperProfileCommand(invalidFullName!, "Headline", null, null);
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("DeveloperProfile.FullName.Empty", result.Error!.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(nameof(DeveloperProfile.FullName), result.Error.Field);
    }

    [Fact]
    public async Task HandleAsync_WhenFullNameExceedsMaxLength_ReturnsFullNameTooLongError()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Original Name", "Original Headline");
        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        var excessiveName = new string('A', DeveloperProfile.MaxFullNameLength + 1);
        var command = new UpdateDeveloperProfileCommand(excessiveName, "Headline", null, null);
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("DeveloperProfile.FullName.TooLong", result.Error!.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(nameof(DeveloperProfile.FullName), result.Error.Field);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HandleAsync_WhenHeadlineEmptyOrWhitespace_ReturnsEmptyHeadlineError(string? invalidHeadline)
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Original Name", "Original Headline");
        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        var command = new UpdateDeveloperProfileCommand("Ada Lovelace", invalidHeadline!, null, null);
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("DeveloperProfile.Headline.Empty", result.Error!.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(nameof(DeveloperProfile.Headline), result.Error.Field);
    }

    [Fact]
    public async Task HandleAsync_WhenHeadlineExceedsMaxLength_ReturnsHeadlineTooLongError()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Original Name", "Original Headline");
        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        var excessiveHeadline = new string('H', DeveloperProfile.MaxHeadlineLength + 1);
        var command = new UpdateDeveloperProfileCommand("Ada Lovelace", excessiveHeadline, null, null);
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("DeveloperProfile.Headline.TooLong", result.Error!.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(nameof(DeveloperProfile.Headline), result.Error.Field);
    }

    [Fact]
    public async Task HandleAsync_WhenLocationExceedsMaxLength_ReturnsLocationTooLongError()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Original Name", "Original Headline");
        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        var excessiveLocation = new string('L', DeveloperProfile.MaxLocationLength + 1);
        var command = new UpdateDeveloperProfileCommand("Ada Lovelace", "Headline", null, excessiveLocation);
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("DeveloperProfile.Location.TooLong", result.Error!.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(nameof(DeveloperProfile.Location), result.Error.Field);
    }

    [Fact]
    public async Task HandleAsync_WhenValidCommand_UpdatesProfileAndSaves()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Original Name", "Original Headline");
        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        var command = new UpdateDeveloperProfileCommand("  Ada Lovelace  ", "  Lead Architect  ", "  Bio text  ", "  London, UK  ");
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Equal("Ada Lovelace", profile.FullName);
        Assert.Equal("Lead Architect", profile.Headline);
        Assert.Equal("Bio text", profile.Bio);
        Assert.Equal("London, UK", profile.Location);
        Assert.NotNull(profile.UpdatedAtUtc);
        Assert.Equal(1, dbContext.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_WhenEmptyBioAndLocation_NormalizesToNull()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Original Name", "Original Headline");
        profile.UpdateBasicInformation("Original Name", "Original Headline", "Existing bio", "Existing location");

        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        var command = new UpdateDeveloperProfileCommand("Ada Lovelace", "Lead Architect", "   ", "");
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Null(profile.Bio);
        Assert.Null(profile.Location);
    }

    [Fact]
    public async Task HandleAsync_WhenValuesUnchanged_ReturnsSuccessNoOp()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Lead Architect");
        var dbContext = new TestApplicationDbContext();
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        var command = new UpdateDeveloperProfileCommand("Ada Lovelace", "Lead Architect", null, null);
        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Null(profile.UpdatedAtUtc);
        Assert.Equal(1, dbContext.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_WhenDbUpdateConcurrencyExceptionOccurs_ReturnsConflictError()
    {
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Original Name", "Original Headline");
        var dbContext = new TestApplicationDbContext
        {
            SaveChangesBehavior = _ => throw new DbUpdateConcurrencyException("Stale row version.")
        };
        dbContext.DeveloperProfiles.Data.Add(profile);

        var tenantContext = TestTenantContext.AuthenticatedWithTenant(tenantId);
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        var command = new UpdateDeveloperProfileCommand("Ada Lovelace", "Lead Architect", null, null);
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
        var handler = new UpdateDeveloperProfileCommandHandler(dbContext, tenantContext);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new UpdateDeveloperProfileCommand("Ada Lovelace", "Lead Architect", null, null);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.HandleAsync(command, cts.Token));
    }
}
