using Profolio.Domain.ValueObjects;

namespace Profolio.UnitTests;

public class EmailAddressTests
{
    [Fact]
    public void Constructor_WithValidEmail_CreatesEmailAddress()
    {
        // Arrange
        var rawEmail = "developer@example.com";

        // Act
        var email = new EmailAddress(rawEmail);

        // Assert
        Assert.Equal("developer@example.com", email.Value);
    }

    [Fact]
    public void Constructor_WithUntrimmedEmail_TrimsLeadingAndTrailingWhitespace()
    {
        // Arrange
        var rawEmail = "   developer@example.com   ";

        // Act
        var email = new EmailAddress(rawEmail);

        // Assert
        Assert.Equal("developer@example.com", email.Value);
    }

    [Fact]
    public void Constructor_NormalizesDomainToLowercase_AndPreservesLocalPartCasing()
    {
        // Arrange
        var rawEmail = "Ada.Lovelace@EXAMPLE.COM";

        // Act
        var email = new EmailAddress(rawEmail);

        // Assert
        Assert.Equal("Ada.Lovelace@example.com", email.Value);
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullEmail = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EmailAddress(nullEmail!));
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
        var exception = Assert.Throws<ArgumentException>(() => new EmailAddress(invalidInput));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("user@@domain.com")]
    [InlineData("user@domain@com")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    public void Constructor_WithMissingOrMultipleAtOrMissingParts_ThrowsArgumentException(string malformedEmail)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new EmailAddress(malformedEmail));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("user@localhost")]
    [InlineData("user@domain")]
    public void Constructor_WithMissingDomainDot_ThrowsArgumentException(string emailWithoutDot)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new EmailAddress(emailWithoutDot));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("user@domain..com")]
    [InlineData("user@.domain.com")]
    [InlineData("user@domain.com.")]
    [InlineData("user@-domain.com")]
    [InlineData("user@domain-.com")]
    [InlineData("user@sub.-domain.com")]
    [InlineData("user@sub.domain-.com")]
    public void Constructor_WithInvalidDomainLabels_ThrowsArgumentException(string invalidDomainEmail)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new EmailAddress(invalidDomainEmail));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("user name@domain.com")]
    [InlineData("user@ domain.com")]
    [InlineData("user@dom ain.com")]
    public void Constructor_WithInternalWhitespace_ThrowsArgumentException(string emailWithWhitespace)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new EmailAddress(emailWithWhitespace));
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithMaximumPermittedLength254_Succeeds()
    {
        // Arrange
        // localPart (64 chars) + "@" (1) + domain (189 chars: 185 'b' + ".com") = 254 chars total
        var localPart = new string('a', 64);
        var domainPart = new string('b', 185) + ".com";
        var emailStr = $"{localPart}@{domainPart}";
        Assert.Equal(254, emailStr.Length);

        // Act
        var email = new EmailAddress(emailStr);

        // Assert
        Assert.Equal(254, email.Value.Length);
    }

    [Fact]
    public void Constructor_ExceedingMaximumLength254_ThrowsArgumentException()
    {
        // Arrange
        // 65 local + 1 @ + 189 domain = 255 characters
        var localPart = new string('a', 65);
        var domainPart = new string('b', 185) + ".com";
        var emailStr = $"{localPart}@{domainPart}";
        Assert.Equal(255, emailStr.Length);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new EmailAddress(emailStr));
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Equality_EquivalentNormalizedAddresses_AreEqual()
    {
        // Arrange
        var email1 = new EmailAddress("Ada.Lovelace@EXAMPLE.com");
        var email2 = new EmailAddress("Ada.Lovelace@example.com");

        // Act & Assert
        Assert.Equal(email1, email2);
        Assert.True(email1 == email2);
        Assert.Equal(email1.GetHashCode(), email2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentLocalPartCasing_AreNotEqual()
    {
        // Arrange - local part casing is preserved
        var email1 = new EmailAddress("ada.lovelace@example.com");
        var email2 = new EmailAddress("Ada.Lovelace@example.com");

        // Act & Assert
        Assert.NotEqual(email1, email2);
        Assert.False(email1 == email2);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var email = new EmailAddress("Ada.Lovelace@example.com");

        // Act & Assert
        Assert.Equal("Ada.Lovelace@example.com", email.ToString());
    }

    [Fact]
    public void Constructor_WithInternalDomainHyphen_Succeeds()
    {
        // Arrange
        var rawEmail = "developer@my-portfolio.com";

        // Act
        var email = new EmailAddress(rawEmail);

        // Assert
        Assert.Equal("developer@my-portfolio.com", email.Value);
    }
}
