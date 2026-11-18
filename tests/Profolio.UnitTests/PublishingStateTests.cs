using System.Reflection;
using Profolio.Domain.Entities;

namespace Profolio.UnitTests;

public class PublishingStateTests
{
    private const string SampleSnapshotJson = "{\"profile\":{\"name\":\"Ada Lovelace\"}}";

    [Fact]
    public void Constructor_WithValidTenantId_InitializesUnpublishedState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var state = new PublishingState(tenantId);

        // Assert
        Assert.NotEqual(Guid.Empty, state.Id);
        Assert.Equal(tenantId, state.TenantId);
        Assert.False(state.IsPublished);
        Assert.Null(state.ActivePortfolioVersionId);
        Assert.Null(state.ActiveVersionNumber);
        Assert.Null(state.PublishedAtUtc);
        Assert.Null(state.UnpublishedAtUtc);
        Assert.True(state.CreatedAtUtc <= DateTime.UtcNow);
        Assert.Null(state.UpdatedAtUtc);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PublishingState(Guid.Empty));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public void Publish_WithValidVersion_UpdatesActivePointerAndSetsTimestamps()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var state = new PublishingState(tenantId);
        var version = new PortfolioVersion(tenantId, 1, SampleSnapshotJson);

        // Act
        state.Publish(version);

        // Assert
        Assert.True(state.IsPublished);
        Assert.Equal(version.Id, state.ActivePortfolioVersionId);
        Assert.Equal(1, state.ActiveVersionNumber);
        Assert.NotNull(state.PublishedAtUtc);
        Assert.True(state.PublishedAtUtc >= state.CreatedAtUtc);
        Assert.Null(state.UnpublishedAtUtc);
        Assert.NotNull(state.UpdatedAtUtc);
        Assert.True(state.UpdatedAtUtc >= state.CreatedAtUtc);
    }

    [Fact]
    public void Publish_WithNullVersion_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var state = new PublishingState(tenantId);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => state.Publish(null!));
        Assert.Equal("version", exception.ParamName);
        Assert.False(state.IsPublished);
        Assert.Null(state.ActivePortfolioVersionId);
        Assert.Null(state.ActiveVersionNumber);
        Assert.Null(state.PublishedAtUtc);
        Assert.Null(state.UnpublishedAtUtc);
        Assert.Null(state.UpdatedAtUtc);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_WithNullVersion_ThrowsAndPreservesExistingActiveVersion()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var state = new PublishingState(tenantId);
        var validVersion = new PortfolioVersion(tenantId, 1, SampleSnapshotJson);
        state.Publish(validVersion);
        var originalPublishedAt = state.PublishedAtUtc;
        var originalUpdatedAt = state.UpdatedAtUtc;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => state.Publish(null!));
        Assert.Equal("version", exception.ParamName);
        Assert.True(state.IsPublished);
        Assert.Equal(validVersion.Id, state.ActivePortfolioVersionId);
        Assert.Equal(1, state.ActiveVersionNumber);
        Assert.Equal(originalPublishedAt, state.PublishedAtUtc);
        Assert.Null(state.UnpublishedAtUtc);
        Assert.Equal(originalUpdatedAt, state.UpdatedAtUtc);
    }

    [Fact]
    public void Publish_WithVersionFromDifferentTenant_ThrowsArgumentExceptionAndPreservesState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var foreignTenantId = Guid.NewGuid();
        var state = new PublishingState(tenantId);
        var foreignVersion = new PortfolioVersion(foreignTenantId, 1, SampleSnapshotJson);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => state.Publish(foreignVersion));
        Assert.Equal("version", exception.ParamName);
        Assert.Contains("does not belong to this tenant", exception.Message);
        Assert.False(state.IsPublished);
        Assert.Null(state.ActivePortfolioVersionId);
        Assert.Null(state.ActiveVersionNumber);
        Assert.Null(state.PublishedAtUtc);
        Assert.Null(state.UnpublishedAtUtc);
        Assert.Null(state.UpdatedAtUtc);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_WithVersionFromDifferentTenant_ThrowsAndPreservesExistingActiveVersion()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var foreignTenantId = Guid.NewGuid();
        var state = new PublishingState(tenantId);
        var validVersion = new PortfolioVersion(tenantId, 1, SampleSnapshotJson);
        state.Publish(validVersion);
        var originalUpdatedAt = state.UpdatedAtUtc;
        var originalPublishedAt = state.PublishedAtUtc;

        var foreignVersion = new PortfolioVersion(foreignTenantId, 2, SampleSnapshotJson);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => state.Publish(foreignVersion));
        Assert.Equal("version", exception.ParamName);
        Assert.True(state.IsPublished);
        Assert.Equal(validVersion.Id, state.ActivePortfolioVersionId);
        Assert.Equal(1, state.ActiveVersionNumber);
        Assert.Equal(originalPublishedAt, state.PublishedAtUtc);
        Assert.Null(state.UnpublishedAtUtc);
        Assert.Equal(originalUpdatedAt, state.UpdatedAtUtc);
    }

    [Fact]
    public void Publish_WhenAlreadyPublishedWithSameVersion_IsIdempotentAndPreservesTimestamps()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var state = new PublishingState(tenantId);
        var version = new PortfolioVersion(tenantId, 1, SampleSnapshotJson);
        state.Publish(version);
        var originalUpdatedAt = state.UpdatedAtUtc;
        var originalPublishedAt = state.PublishedAtUtc;

        // Act - publish identical version again
        state.Publish(version);

        // Assert
        Assert.True(state.IsPublished);
        Assert.Equal(version.Id, state.ActivePortfolioVersionId);
        Assert.Equal(1, state.ActiveVersionNumber);
        Assert.Equal(originalPublishedAt, state.PublishedAtUtc);
        Assert.Equal(originalUpdatedAt, state.UpdatedAtUtc);
    }

    [Fact]
    public void Publish_RepublishingWithNewVersion_UpdatesPointerAndRefreshesTimestamps()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var state = new PublishingState(tenantId);
        var version1 = new PortfolioVersion(tenantId, 1, SampleSnapshotJson);
        var version2 = new PortfolioVersion(tenantId, 2, SampleSnapshotJson);
        state.Publish(version1);
        var firstPublishedAt = state.PublishedAtUtc;
        var firstUpdatedAt = state.UpdatedAtUtc;
        Assert.NotNull(firstPublishedAt);
        Assert.NotNull(firstUpdatedAt);

        // Act
        state.Publish(version2);

        // Assert
        Assert.True(state.IsPublished);
        Assert.Equal(version2.Id, state.ActivePortfolioVersionId);
        Assert.Equal(2, state.ActiveVersionNumber);
        Assert.NotNull(state.PublishedAtUtc);
        Assert.True(state.PublishedAtUtc >= firstPublishedAt);
        Assert.Null(state.UnpublishedAtUtc);
        Assert.NotNull(state.UpdatedAtUtc);
        Assert.True(state.UpdatedAtUtc >= firstUpdatedAt);
    }

    [Fact]
    public void Unpublish_WhenPublished_ClearsActivePointerAndSetsUnpublishedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var state = new PublishingState(tenantId);
        var version = new PortfolioVersion(tenantId, 1, SampleSnapshotJson);
        state.Publish(version);
        var publishedAt = state.PublishedAtUtc;
        var beforeUnpublishUpdatedAt = state.UpdatedAtUtc;
        Assert.NotNull(publishedAt);
        Assert.NotNull(beforeUnpublishUpdatedAt);

        // Act
        state.Unpublish();

        // Assert
        Assert.False(state.IsPublished);
        Assert.Null(state.ActivePortfolioVersionId);
        Assert.Null(state.ActiveVersionNumber);
        Assert.NotNull(state.UnpublishedAtUtc);
        Assert.True(state.UnpublishedAtUtc >= publishedAt);
        Assert.Equal(publishedAt, state.PublishedAtUtc); // Retains last successful publication timestamp
        Assert.NotNull(state.UpdatedAtUtc);
        Assert.True(state.UpdatedAtUtc >= beforeUnpublishUpdatedAt);
    }

    [Fact]
    public void Unpublish_WhenInitiallyUnpublished_DoesNotModifyStateOrUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var state = new PublishingState(tenantId);

        // Act
        state.Unpublish();

        // Assert
        Assert.False(state.IsPublished);
        Assert.Null(state.ActivePortfolioVersionId);
        Assert.Null(state.ActiveVersionNumber);
        Assert.Null(state.PublishedAtUtc);
        Assert.Null(state.UnpublishedAtUtc);
        Assert.Null(state.UpdatedAtUtc);
    }

    [Fact]
    public void Unpublish_WhenAlreadyUnpublishedAfterPublish_IsIdempotentAndPreservesTimestamps()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var state = new PublishingState(tenantId);
        var version = new PortfolioVersion(tenantId, 1, SampleSnapshotJson);
        state.Publish(version);
        state.Unpublish();

        var originalPublishedAt = state.PublishedAtUtc;
        var originalUnpublishedAt = state.UnpublishedAtUtc;
        var originalUpdatedAt = state.UpdatedAtUtc;

        // Act - call unpublish again
        state.Unpublish();

        // Assert
        Assert.False(state.IsPublished);
        Assert.Null(state.ActivePortfolioVersionId);
        Assert.Null(state.ActiveVersionNumber);
        Assert.Equal(originalPublishedAt, state.PublishedAtUtc);
        Assert.Equal(originalUnpublishedAt, state.UnpublishedAtUtc);
        Assert.Equal(originalUpdatedAt, state.UpdatedAtUtc);
    }

    [Fact]
    public void Publish_AfterUnpublishing_RestoresPublishedStateAndClearsUnpublishedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var state = new PublishingState(tenantId);
        var version1 = new PortfolioVersion(tenantId, 1, SampleSnapshotJson);
        var version2 = new PortfolioVersion(tenantId, 2, SampleSnapshotJson);
        state.Publish(version1);
        state.Unpublish();
        var previousUnpublishedAt = state.UnpublishedAtUtc;
        var previousUpdatedAt = state.UpdatedAtUtc;
        Assert.NotNull(previousUnpublishedAt);
        Assert.NotNull(previousUpdatedAt);

        // Act - re-publish with version 2
        state.Publish(version2);

        // Assert
        Assert.True(state.IsPublished);
        Assert.Equal(version2.Id, state.ActivePortfolioVersionId);
        Assert.Equal(2, state.ActiveVersionNumber);
        Assert.NotNull(state.PublishedAtUtc);
        Assert.True(state.PublishedAtUtc >= previousUnpublishedAt);
        Assert.Null(state.UnpublishedAtUtc); // Cleared upon new publication
        Assert.NotNull(state.UpdatedAtUtc);
        Assert.True(state.UpdatedAtUtc >= previousUpdatedAt);
    }

    [Fact]
    public void Publish_SameVersionAfterUnpublishing_ReactivatesPublishedStateAndRefreshesTimestamps()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var state = new PublishingState(tenantId);
        var version = new PortfolioVersion(tenantId, 1, SampleSnapshotJson);
        state.Publish(version);
        state.Unpublish();
        var previousUnpublishedAt = state.UnpublishedAtUtc;
        var previousUpdatedAt = state.UpdatedAtUtc;
        Assert.NotNull(previousUnpublishedAt);
        Assert.NotNull(previousUpdatedAt);

        // Act - re-publish the exact same version after unpublishing
        state.Publish(version);

        // Assert
        Assert.True(state.IsPublished);
        Assert.Equal(version.Id, state.ActivePortfolioVersionId);
        Assert.Equal(version.VersionNumber, state.ActiveVersionNumber);
        Assert.Null(state.UnpublishedAtUtc);
        Assert.NotNull(state.PublishedAtUtc);
        Assert.True(state.PublishedAtUtc >= previousUnpublishedAt);
        Assert.NotNull(state.UpdatedAtUtc);
        Assert.True(state.UpdatedAtUtc >= previousUpdatedAt);
    }

    [Fact]
    public void TenantId_CannotBeChangedPublicly()
    {
        // Arrange
        var property = typeof(PublishingState).GetProperty("TenantId");

        // Assert
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }

    [Fact]
    public void Security_DoesNotExposeUnsafeOverloadAcceptingBareGuidAndInt()
    {
        // Arrange & Assert
        // Verify no public overload of Publish exists that bypasses PortfolioVersion validation
        var methods = typeof(PublishingState).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == "Publish")
            .ToList();

        Assert.Single(methods);
        var singlePublishMethod = methods[0];
        var parameters = singlePublishMethod.GetParameters();
        Assert.Single(parameters);
        Assert.Equal(typeof(PortfolioVersion), parameters[0].ParameterType);
    }
}
