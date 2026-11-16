using Profolio.Domain.ValueObjects;

namespace Profolio.UnitTests;

public class SocialPlatformTests
{
    [Theory]
    [InlineData("GitHub", "GitHub")]
    [InlineData("LinkedIn", "LinkedIn")]
    [InlineData("X", "X")]
    [InlineData("YouTube", "YouTube")]
    [InlineData("Instagram", "Instagram")]
    [InlineData("StackOverflow", "StackOverflow")]
    [InlineData("PersonalWebsite", "PersonalWebsite")]
    [InlineData("Other", "Other")]
    public void Constructor_WithSupportedPlatform_CreatesSocialPlatform(string input, string expectedCanonical)
    {
        // Act
        var platform = new SocialPlatform(input);

        // Assert
        Assert.Equal(expectedCanonical, platform.Value);
        Assert.Equal(expectedCanonical, platform.ToString());
    }

    [Theory]
    [InlineData("github", "GitHub")]
    [InlineData("GITHUB", "GitHub")]
    [InlineData("linkedin", "LinkedIn")]
    [InlineData("LINKEDIN", "LinkedIn")]
    [InlineData("x", "X")]
    [InlineData("youtube", "YouTube")]
    [InlineData("YOUTUBE", "YouTube")]
    [InlineData("instagram", "Instagram")]
    [InlineData("INSTAGRAM", "Instagram")]
    [InlineData("stackoverflow", "StackOverflow")]
    [InlineData("STACKOVERFLOW", "StackOverflow")]
    [InlineData("personalwebsite", "PersonalWebsite")]
    [InlineData("PERSONALWEBSITE", "PersonalWebsite")]
    [InlineData("other", "Other")]
    [InlineData("OTHER", "Other")]
    public void Constructor_WithCaseInsensitiveKnownPlatform_CanonicalizesName(string input, string expectedCanonical)
    {
        // Act
        var platform = new SocialPlatform(input);

        // Assert
        Assert.Equal(expectedCanonical, platform.Value);
    }

    [Theory]
    [InlineData("   GitHub   ", "GitHub")]
    [InlineData("  twitter  ", "X")]
    [InlineData(" website ", "PersonalWebsite")]
    [InlineData("  other  ", "Other")]
    public void Constructor_WithUntrimmedInput_TrimsWhitespace(string input, string expectedCanonical)
    {
        // Act
        var platform = new SocialPlatform(input);

        // Assert
        Assert.Equal(expectedCanonical, platform.Value);
    }

    [Theory]
    [InlineData("twitter", "X")]
    [InlineData("Twitter", "X")]
    [InlineData("TWITTER", "X")]
    public void Constructor_WithTwitterAlias_NormalizesToX(string input, string expectedCanonical)
    {
        // Act
        var platform = new SocialPlatform(input);

        // Assert
        Assert.Equal(expectedCanonical, platform.Value);
        Assert.Equal(SocialPlatform.X, platform);
    }

    [Theory]
    [InlineData("website", "PersonalWebsite")]
    [InlineData("Website", "PersonalWebsite")]
    [InlineData("WEBSITE", "PersonalWebsite")]
    public void Constructor_WithWebsiteAlias_NormalizesToPersonalWebsite(string input, string expectedCanonical)
    {
        // Act
        var platform = new SocialPlatform(input);

        // Assert
        Assert.Equal(expectedCanonical, platform.Value);
        Assert.Equal(SocialPlatform.PersonalWebsite, platform);
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullInput = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new SocialPlatform(nullInput!));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithEmptyOrWhitespace_ThrowsArgumentException(string invalidInput)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new SocialPlatform(invalidInput));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("Facebook")]
    [InlineData("Twitch")]
    [InlineData("DevTo")]
    [InlineData("Medium")]
    [InlineData("Discord")]
    [InlineData("GitLab")]
    [InlineData("Mastodon")]
    [InlineData("UnknownPlatform")]
    public void Constructor_WithUnknownPlatform_ThrowsArgumentException(string unknownPlatform)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new SocialPlatform(unknownPlatform));
        Assert.Equal("value", exception.ParamName);
        Assert.Contains("Unsupported social platform", exception.Message);
    }

    [Fact]
    public void Constructor_WithInputExceedingMaxLength_ThrowsArgumentException()
    {
        // Arrange
        var excessiveInput = new string('A', SocialPlatform.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new SocialPlatform(excessiveInput));
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void RequiresLabel_WhenOther_ReturnsTrue()
    {
        // Arrange & Act
        var otherStatic = SocialPlatform.Other;
        var otherConstructed = new SocialPlatform("other");

        // Assert
        Assert.True(otherStatic.RequiresLabel);
        Assert.True(otherConstructed.RequiresLabel);
    }

    [Theory]
    [InlineData("GitHub")]
    [InlineData("LinkedIn")]
    [InlineData("X")]
    [InlineData("YouTube")]
    [InlineData("Instagram")]
    [InlineData("StackOverflow")]
    [InlineData("PersonalWebsite")]
    public void RequiresLabel_WhenNotOther_ReturnsFalse(string platformName)
    {
        // Arrange & Act
        var platform = new SocialPlatform(platformName);

        // Assert
        Assert.False(platform.RequiresLabel);
    }

    [Fact]
    public void StaticInstances_ExposeAllExpectedSupportedPlatforms()
    {
        // Assert
        Assert.Equal("GitHub", SocialPlatform.GitHub.Value);
        Assert.Equal("LinkedIn", SocialPlatform.LinkedIn.Value);
        Assert.Equal("X", SocialPlatform.X.Value);
        Assert.Equal("YouTube", SocialPlatform.YouTube.Value);
        Assert.Equal("Instagram", SocialPlatform.Instagram.Value);
        Assert.Equal("StackOverflow", SocialPlatform.StackOverflow.Value);
        Assert.Equal("PersonalWebsite", SocialPlatform.PersonalWebsite.Value);
        Assert.Equal("Other", SocialPlatform.Other.Value);
    }

    [Fact]
    public void ValueEquality_WithEquivalentPlatforms_EvaluatesAsEqual()
    {
        // Arrange
        var p1 = new SocialPlatform("github");
        var p2 = new SocialPlatform("GITHUB");
        var p3 = SocialPlatform.GitHub;

        // Act & Assert
        Assert.True(p1 == p2);
        Assert.True(p1 == p3);
        Assert.True(p1.Equals(p2));
        Assert.True(p1.Equals(p3));
        Assert.Equal(p1.GetHashCode(), p2.GetHashCode());
        Assert.Equal(p1.GetHashCode(), p3.GetHashCode());
    }

    [Fact]
    public void ValueEquality_WithDifferentPlatforms_EvaluatesAsNotEqual()
    {
        // Arrange
        var p1 = SocialPlatform.GitHub;
        var p2 = SocialPlatform.LinkedIn;

        // Act & Assert
        Assert.False(p1 == p2);
        Assert.True(p1 != p2);
        Assert.False(p1.Equals(p2));
    }

    [Fact]
    public void ValueEquality_WithNull_ReturnsFalse()
    {
        // Arrange
        var platform = SocialPlatform.GitHub;

        // Act & Assert
        Assert.False(platform.Equals(null));
        Assert.False(platform == null);
        Assert.False(null == platform);
        Assert.True(platform != null);
    }
}
