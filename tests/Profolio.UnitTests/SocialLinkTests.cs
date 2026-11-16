using Profolio.Domain.Common;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;

namespace Profolio.UnitTests;

public class SocialLinkTests
{
    // =========================================================================
    // CONSTRUCTION TESTS
    // =========================================================================

    [Fact]
    public void Constructor_WithAllValidArguments_CreatesSocialLink()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var platform = SocialPlatform.GitHub;
        var url = new WebUrl("https://github.com/developer");
        var label = "Primary GitHub";
        var displayOrder = 1;

        // Act
        var link = new SocialLink(tenantId, platform, url, label, displayOrder);

        // Assert
        Assert.NotEqual(Guid.Empty, link.Id);
        Assert.Equal(tenantId, link.TenantId);
        Assert.Equal(platform, link.Platform);
        Assert.Equal(url, link.Url);
        Assert.Equal(label, link.Label);
        Assert.Equal(displayOrder, link.DisplayOrder);
        Assert.True(link.CreatedAtUtc <= DateTime.UtcNow);
        Assert.Null(link.UpdatedAtUtc);
        Assert.IsAssignableFrom<ITenantEntity>(link);
        Assert.IsAssignableFrom<BaseEntity>(link);
    }

    [Fact]
    public void Constructor_WithDefaultOptionalArguments_SetsDefaults()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var platform = SocialPlatform.LinkedIn;
        var url = new WebUrl("https://linkedin.com/in/developer");

        // Act
        var link = new SocialLink(tenantId, platform, url);

        // Assert
        Assert.Equal(tenantId, link.TenantId);
        Assert.Equal(platform, link.Platform);
        Assert.Equal(url, link.Url);
        Assert.Null(link.Label);
        Assert.Equal(0, link.DisplayOrder);
        Assert.Null(link.UpdatedAtUtc);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;
        var platform = SocialPlatform.GitHub;
        var url = new WebUrl("https://github.com/developer");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new SocialLink(emptyTenantId, platform, url));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullPlatform_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var url = new WebUrl("https://github.com/developer");

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new SocialLink(tenantId, null!, url));
        Assert.Equal("platform", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullUrl_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var platform = SocialPlatform.GitHub;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new SocialLink(tenantId, platform, null!));
        Assert.Equal("url", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceLabelOnStandardPlatform_SetsLabelToNull(string? rawLabel)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var platform = SocialPlatform.GitHub;
        var url = new WebUrl("https://github.com/developer");

        // Act
        var link = new SocialLink(tenantId, platform, url, rawLabel);

        // Assert
        Assert.Null(link.Label);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceLabelOnPersonalWebsite_SetsLabelToNull(string? rawLabel)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var platform = SocialPlatform.PersonalWebsite;
        var url = new WebUrl("https://developer.dev");

        // Act
        var link = new SocialLink(tenantId, platform, url, rawLabel);

        // Assert
        Assert.Null(link.Label);
    }

    [Fact]
    public void Constructor_WithUntrimmedLabel_TrimsWhitespace()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var platform = SocialPlatform.GitHub;
        var url = new WebUrl("https://github.com/developer");
        var rawLabel = "   My Open Source   ";

        // Act
        var link = new SocialLink(tenantId, platform, url, rawLabel);

        // Assert
        Assert.Equal("My Open Source", link.Label);
    }

    [Fact]
    public void Constructor_WithLabelAtMaximumLength_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var platform = SocialPlatform.GitHub;
        var url = new WebUrl("https://github.com/developer");
        var maxLabel = new string('L', SocialLink.MaxLabelLength);

        // Act
        var link = new SocialLink(tenantId, platform, url, maxLabel);

        // Assert
        Assert.NotNull(link.Label);
        Assert.Equal(maxLabel, link.Label);
        Assert.Equal(SocialLink.MaxLabelLength, link.Label.Length);
    }

    [Fact]
    public void Constructor_WithLabelAtMaximumLengthSurroundedByWhitespace_TrimsAndSucceeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var platform = SocialPlatform.GitHub;
        var url = new WebUrl("https://github.com/developer");
        var maxLabel = new string('L', SocialLink.MaxLabelLength);
        var paddedLabel = $"   {maxLabel}   ";

        // Act
        var link = new SocialLink(tenantId, platform, url, paddedLabel);

        // Assert
        Assert.NotNull(link.Label);
        Assert.Equal(maxLabel, link.Label);
        Assert.Equal(SocialLink.MaxLabelLength, link.Label.Length);
    }

    [Fact]
    public void Constructor_WithLabelExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var platform = SocialPlatform.GitHub;
        var url = new WebUrl("https://github.com/developer");
        var excessiveLabel = new string('L', SocialLink.MaxLabelLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new SocialLink(tenantId, platform, url, excessiveLabel));
        Assert.Equal("label", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WhenPlatformIsOtherAndLabelNullOrWhitespace_ThrowsArgumentException(string? invalidLabel)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var platform = SocialPlatform.Other;
        var url = new WebUrl("https://dev.to/developer");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new SocialLink(tenantId, platform, url, invalidLabel));
        Assert.Equal("label", exception.ParamName);
        Assert.Contains("Label is required", exception.Message);
    }

    [Fact]
    public void Constructor_WhenPlatformIsOtherAndLabelProvided_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var platform = SocialPlatform.Other;
        var url = new WebUrl("https://dev.to/developer");
        var label = "Dev.to Profile";

        // Act
        var link = new SocialLink(tenantId, platform, url, label);

        // Assert
        Assert.Equal(platform, link.Platform);
        Assert.Equal(label, link.Label);
    }

    [Fact]
    public void Constructor_WithValidDisplayOrder_SetsDisplayOrder()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var platform = SocialPlatform.X;
        var url = new WebUrl("https://x.com/developer");

        // Act
        var link = new SocialLink(tenantId, platform, url, displayOrder: 4);

        // Assert
        Assert.Equal(4, link.DisplayOrder);
    }

    [Fact]
    public void Constructor_WithNegativeDisplayOrder_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var platform = SocialPlatform.X;
        var url = new WebUrl("https://x.com/developer");

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new SocialLink(tenantId, platform, url, displayOrder: -1));
        Assert.Equal("displayOrder", exception.ParamName);
    }

    // =========================================================================
    // UPDATE TESTS
    // =========================================================================

    [Fact]
    public void UpdateDetails_WithAllNewValidValues_UpdatesAllPropertiesAndSetsUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var link = new SocialLink(
            tenantId,
            SocialPlatform.GitHub,
            new WebUrl("https://github.com/developer"),
            "Old Label",
            0);

        var newPlatform = SocialPlatform.LinkedIn;
        var newUrl = new WebUrl("https://linkedin.com/in/developer");
        var newLabel = "Updated LinkedIn";
        var newDisplayOrder = 2;

        // Act
        link.UpdateDetails(newPlatform, newUrl, newLabel, newDisplayOrder);

        // Assert
        Assert.Equal(newPlatform, link.Platform);
        Assert.Equal(newUrl, link.Url);
        Assert.Equal(newLabel, link.Label);
        Assert.Equal(newDisplayOrder, link.DisplayOrder);
        Assert.NotNull(link.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_UpdatingPlatformAlone_UpdatesUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var url = new WebUrl("https://example.com/profile");
        var link = new SocialLink(tenantId, SocialPlatform.X, url, "Handle", 1);

        // Act
        link.UpdateDetails(SocialPlatform.YouTube, url, "Handle", 1);

        // Assert
        Assert.Equal(SocialPlatform.YouTube, link.Platform);
        Assert.NotNull(link.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_UpdatingUrlAlone_UpdatesUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var oldUrl = new WebUrl("https://github.com/old");
        var newUrl = new WebUrl("https://github.com/new");
        var link = new SocialLink(tenantId, SocialPlatform.GitHub, oldUrl, null, 0);

        // Act
        link.UpdateDetails(SocialPlatform.GitHub, newUrl, null, 0);

        // Assert
        Assert.Equal(newUrl, link.Url);
        Assert.NotNull(link.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_UpdatingLabelAlone_UpdatesUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var url = new WebUrl("https://github.com/developer");
        var link = new SocialLink(tenantId, SocialPlatform.GitHub, url, "Initial", 0);

        // Act
        link.UpdateDetails(SocialPlatform.GitHub, url, "Changed", 0);

        // Assert
        Assert.Equal("Changed", link.Label);
        Assert.NotNull(link.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_ClearingOptionalLabel_SetsLabelToNullAndUpdatesUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var url = new WebUrl("https://github.com/developer");
        var link = new SocialLink(tenantId, SocialPlatform.GitHub, url, "Some Label", 0);

        // Act
        link.UpdateDetails(SocialPlatform.GitHub, url, "   ", 0);

        // Assert
        Assert.Null(link.Label);
        Assert.NotNull(link.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_UpdatingDisplayOrderAlone_UpdatesUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var url = new WebUrl("https://github.com/developer");
        var link = new SocialLink(tenantId, SocialPlatform.GitHub, url, null, 1);

        // Act
        link.UpdateDetails(SocialPlatform.GitHub, url, null, 3);

        // Assert
        Assert.Equal(3, link.DisplayOrder);
        Assert.NotNull(link.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WithIdenticalNormalizedValues_DoesNotModifyStateOrUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var url = new WebUrl("https://github.com/developer");
        var link = new SocialLink(tenantId, SocialPlatform.GitHub, url, "My Code", 2);

        // Act - provide same platform, same URL, same label with padding that normalizes identically, same display order
        link.UpdateDetails(new SocialPlatform("github"), new WebUrl("https://github.com/developer"), "  My Code  ", 2);

        // Assert
        Assert.Null(link.UpdatedAtUtc);
        Assert.Equal(SocialPlatform.GitHub, link.Platform);
        Assert.Equal(url, link.Url);
        Assert.Equal("My Code", link.Label);
        Assert.Equal(2, link.DisplayOrder);
    }

    [Fact]
    public void UpdateDetails_PreservesIdAndTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var link = new SocialLink(tenantId, SocialPlatform.GitHub, new WebUrl("https://github.com/original"));
        var originalId = link.Id;
        var originalTenantId = link.TenantId;

        // Act
        link.UpdateDetails(SocialPlatform.LinkedIn, new WebUrl("https://linkedin.com/in/updated"), "New", 5);

        // Assert
        Assert.Equal(originalId, link.Id);
        Assert.Equal(originalTenantId, link.TenantId);
    }

    // =========================================================================
    // ATOMICITY & REGRESSION HARDENING TESTS
    // =========================================================================

    [Fact]
    public void UpdateDetails_WhenLabelExceedsMaxLength_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalPlatform = SocialPlatform.GitHub;
        var originalUrl = new WebUrl("https://github.com/original");
        var originalLabel = "Original Label";
        var originalDisplayOrder = 1;
        var link = new SocialLink(tenantId, originalPlatform, originalUrl, originalLabel, originalDisplayOrder);

        var originalId = link.Id;
        var originalTenantId = link.TenantId;
        var originalUpdatedAt = link.UpdatedAtUtc;
        var excessiveLabel = new string('L', SocialLink.MaxLabelLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => link.UpdateDetails(
            SocialPlatform.LinkedIn,
            new WebUrl("https://linkedin.com/in/new"),
            excessiveLabel,
            3));

        Assert.Equal("label", exception.ParamName);
        Assert.Equal(originalId, link.Id);
        Assert.Equal(originalTenantId, link.TenantId);
        Assert.Equal(originalPlatform, link.Platform);
        Assert.Equal(originalUrl, link.Url);
        Assert.Equal(originalLabel, link.Label);
        Assert.Equal(originalDisplayOrder, link.DisplayOrder);
        Assert.Equal(originalUpdatedAt, link.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenPlatformChangedToOtherAndLabelMissing_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalPlatform = SocialPlatform.GitHub;
        var originalUrl = new WebUrl("https://github.com/original");
        var originalLabel = "Original Label";
        var originalDisplayOrder = 1;
        var link = new SocialLink(tenantId, originalPlatform, originalUrl, originalLabel, originalDisplayOrder);

        var originalId = link.Id;
        var originalTenantId = link.TenantId;
        var originalUpdatedAt = link.UpdatedAtUtc;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => link.UpdateDetails(
            SocialPlatform.Other,
            new WebUrl("https://custom.com/profile"),
            "   ",
            2));

        Assert.Equal("label", exception.ParamName);
        Assert.Equal(originalId, link.Id);
        Assert.Equal(originalTenantId, link.TenantId);
        Assert.Equal(originalPlatform, link.Platform);
        Assert.Equal(originalUrl, link.Url);
        Assert.Equal(originalLabel, link.Label);
        Assert.Equal(originalDisplayOrder, link.DisplayOrder);
        Assert.Equal(originalUpdatedAt, link.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenDisplayOrderNegative_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalPlatform = SocialPlatform.GitHub;
        var originalUrl = new WebUrl("https://github.com/original");
        var originalLabel = "Original Label";
        var originalDisplayOrder = 1;
        var link = new SocialLink(tenantId, originalPlatform, originalUrl, originalLabel, originalDisplayOrder);

        var originalId = link.Id;
        var originalTenantId = link.TenantId;
        var originalUpdatedAt = link.UpdatedAtUtc;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => link.UpdateDetails(
            SocialPlatform.LinkedIn,
            new WebUrl("https://linkedin.com/in/new"),
            "New Label",
            -5));

        Assert.Equal("displayOrder", exception.ParamName);
        Assert.Equal(originalId, link.Id);
        Assert.Equal(originalTenantId, link.TenantId);
        Assert.Equal(originalPlatform, link.Platform);
        Assert.Equal(originalUrl, link.Url);
        Assert.Equal(originalLabel, link.Label);
        Assert.Equal(originalDisplayOrder, link.DisplayOrder);
        Assert.Equal(originalUpdatedAt, link.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenPlatformNull_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalPlatform = SocialPlatform.GitHub;
        var originalUrl = new WebUrl("https://github.com/original");
        var link = new SocialLink(tenantId, originalPlatform, originalUrl, "Original", 1);

        var originalId = link.Id;
        var originalTenantId = link.TenantId;
        var originalUpdatedAt = link.UpdatedAtUtc;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => link.UpdateDetails(
            null!,
            new WebUrl("https://new.dev"),
            "Valid Label",
            2));

        Assert.Equal("platform", exception.ParamName);
        Assert.Equal(originalId, link.Id);
        Assert.Equal(originalTenantId, link.TenantId);
        Assert.Equal(originalPlatform, link.Platform);
        Assert.Equal(originalUrl, link.Url);
        Assert.Equal("Original", link.Label);
        Assert.Equal(1, link.DisplayOrder);
        Assert.Equal(originalUpdatedAt, link.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_WhenUrlNull_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var originalPlatform = SocialPlatform.GitHub;
        var originalUrl = new WebUrl("https://github.com/original");
        var link = new SocialLink(tenantId, originalPlatform, originalUrl, "Original", 1);

        var originalId = link.Id;
        var originalTenantId = link.TenantId;
        var originalUpdatedAt = link.UpdatedAtUtc;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => link.UpdateDetails(
            SocialPlatform.LinkedIn,
            null!,
            "Valid Label",
            2));

        Assert.Equal("url", exception.ParamName);
        Assert.Equal(originalId, link.Id);
        Assert.Equal(originalTenantId, link.TenantId);
        Assert.Equal(originalPlatform, link.Platform);
        Assert.Equal(originalUrl, link.Url);
        Assert.Equal("Original", link.Label);
        Assert.Equal(1, link.DisplayOrder);
        Assert.Equal(originalUpdatedAt, link.UpdatedAtUtc);
    }
}
