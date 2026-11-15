using Profolio.Domain.ValueObjects;

namespace Profolio.UnitTests;

public class UsernameTests
{
    [Fact]
    public void Constructor_WithValidUsername_CreatesUsername()
    {
        // Arrange
        var rawUsername = "john-doe123";

        // Act
        var username = new Username(rawUsername);

        // Assert
        Assert.Equal("john-doe123", username.Value);
    }

    [Fact]
    public void Constructor_WithUppercaseCharacters_NormalizesToLowercase()
    {
        // Arrange
        var rawUsername = "AdaLovelace";

        // Act
        var username = new Username(rawUsername);

        // Assert
        Assert.Equal("adalovelace", username.Value);
    }

    [Fact]
    public void Constructor_WithMinimumLength_Succeeds()
    {
        // Arrange
        var minUsername = "dev";
        Assert.Equal(Username.MinLength, minUsername.Length);

        // Act
        var username = new Username(minUsername);

        // Assert
        Assert.Equal("dev", username.Value);
    }

    [Fact]
    public void Constructor_WithMaximumLength_Succeeds()
    {
        // Arrange
        var maxUsername = new string('a', Username.MaxLength);
        Assert.Equal(30, maxUsername.Length);

        // Act
        var username = new Username(maxUsername);

        // Assert
        Assert.Equal(maxUsername, username.Value);
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Username(null!));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Constructor_WithEmptyOrWhitespace_ThrowsArgumentException(string invalidInput)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Username(invalidInput));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData(" user")]
    [InlineData("user ")]
    [InlineData("user name")]
    public void Constructor_WithWhitespaceAnywhere_ThrowsArgumentException(string whitespaceUsername)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Username(whitespaceUsername));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    public void Constructor_ShorterThanMinimumLength_ThrowsArgumentException(string shortUsername)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Username(shortUsername));
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_LongerThanMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tooLong = new string('a', Username.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Username(tooLong));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("user@name")]
    [InlineData("user.name")]
    [InlineData("user_name")]
    [InlineData("user!name")]
    [InlineData("user#name")]
    public void Constructor_WithInvalidCharacters_ThrowsArgumentException(string invalidCharUsername)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Username(invalidCharUsername));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("adà")]
    [InlineData("user🚀")]
    [InlineData("über")]
    [InlineData("日本語")]
    public void Constructor_WithNonAsciiOrUnicodeCharacters_ThrowsArgumentException(string nonAsciiUsername)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Username(nonAsciiUsername));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("-username")]
    [InlineData("--username")]
    public void Constructor_WithLeadingHyphen_ThrowsArgumentException(string leadingHyphen)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Username(leadingHyphen));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("username-")]
    [InlineData("username--")]
    public void Constructor_WithTrailingHyphen_ThrowsArgumentException(string trailingHyphen)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Username(trailingHyphen));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("user--name")]
    [InlineData("user---name")]
    public void Constructor_WithConsecutiveHyphens_ThrowsArgumentException(string consecutiveHyphens)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Username(consecutiveHyphens));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("api")]
    [InlineData("API")]
    [InlineData("admin")]
    [InlineData("Admin")]
    [InlineData("login")]
    [InlineData("LOGIN")]
    [InlineData("register")]
    [InlineData("Register")]
    [InlineData("dashboard")]
    [InlineData("Dashboard")]
    [InlineData("settings")]
    [InlineData("Settings")]
    [InlineData("support")]
    [InlineData("Support")]
    [InlineData("public")]
    [InlineData("Public")]
    [InlineData("swagger")]
    [InlineData("Swagger")]
    public void Constructor_WithReservedName_ThrowsArgumentException(string reservedName)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Username(reservedName));
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Equality_EquivalentNormalizedUsernames_AreEqual()
    {
        // Arrange
        var username1 = new Username("AdaLovelace");
        var username2 = new Username("adalovelace");

        // Act & Assert
        Assert.Equal(username1, username2);
        Assert.True(username1 == username2);
        Assert.Equal(username1.GetHashCode(), username2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentUsernames_AreNotEqual()
    {
        // Arrange
        var username1 = new Username("developer-1");
        var username2 = new Username("developer-2");

        // Act & Assert
        Assert.NotEqual(username1, username2);
        Assert.False(username1 == username2);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var username = new Username("adalovelace");

        // Act & Assert
        Assert.Equal("adalovelace", username.ToString());
    }
}
