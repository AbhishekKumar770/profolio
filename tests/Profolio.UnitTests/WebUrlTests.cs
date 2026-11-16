using System.Reflection;
using Profolio.Domain.ValueObjects;

namespace Profolio.UnitTests;

public class WebUrlTests
{
    [Theory]
    [InlineData("http://example.com")]
    [InlineData("http://localhost:5000")]
    [InlineData("http://127.0.0.1:8080/app")]
    public void Constructor_WithValidHttpUrl_CreatesWebUrl(string rawUrl)
    {
        // Act
        var webUrl = new WebUrl(rawUrl);

        // Assert
        Assert.NotNull(webUrl.Value);
        Assert.StartsWith("http://", webUrl.Value, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("https://github.com/dotnet/aspnetcore")]
    [InlineData("https://example.com/path?q=1#hash")]
    [InlineData("https://my-domain.co.uk/portfolio")]
    public void Constructor_WithValidHttpsUrl_CreatesWebUrl(string rawUrl)
    {
        // Act
        var webUrl = new WebUrl(rawUrl);

        // Assert
        Assert.NotNull(webUrl.Value);
        Assert.StartsWith("https://", webUrl.Value, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("https://münchen.de", "https://münchen.de/")]
    [InlineData("https://xn--mnchen-3ya.de", "https://xn--mnchen-3ya.de/")]
    public void Constructor_WithInternationalizedDomainName_SucceedsAndNormalizes(string rawUrl, string expectedNormalizedUrl)
    {
        // Act
        var webUrl = new WebUrl(rawUrl);

        // Assert
        Assert.Equal(expectedNormalizedUrl, webUrl.Value);
    }

    [Fact]
    public void Constructor_WithUntrimmedUrl_TrimsLeadingAndTrailingWhitespace()
    {
        // Arrange
        var rawUrl = "   https://example.com/project   ";

        // Act
        var webUrl = new WebUrl(rawUrl);

        // Assert
        Assert.Equal("https://example.com/project", webUrl.Value);
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullUrl = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new WebUrl(nullUrl!));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithEmptyOrWhitespace_ThrowsArgumentException(string invalidUrl)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WebUrl(invalidUrl));
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithUrlExceedingMaxLength_ThrowsArgumentException()
    {
        // Arrange
        var basePart = "https://example.com/";
        var excessiveUrl = basePart + new string('a', WebUrl.MaxLength - basePart.Length + 1);
        Assert.Equal(WebUrl.MaxLength + 1, excessiveUrl.Length);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WebUrl(excessiveUrl));
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithUrlAtMaxLengthBoundary_Succeeds()
    {
        // Arrange
        var basePart = "https://example.com/";
        var maxUrl = basePart + new string('a', WebUrl.MaxLength - basePart.Length);
        Assert.Equal(WebUrl.MaxLength, maxUrl.Length);

        // Act
        var webUrl = new WebUrl(maxUrl);

        // Assert
        Assert.Equal(maxUrl, webUrl.Value);
    }

    [Theory]
    [InlineData("/relative/path")]
    [InlineData("relative/path")]
    [InlineData("../relative")]
    [InlineData("project.html")]
    [InlineData("?query=1")]
    [InlineData("#section")]
    public void Constructor_WithRelativeUrl_ThrowsArgumentException(string relativeUrl)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WebUrl(relativeUrl));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("http//bad")]
    [InlineData("http:/example.com")]
    [InlineData("not a url")]
    [InlineData("http:// /")]
    [InlineData("://example.com")]
    public void Constructor_WithMalformedUrl_ThrowsArgumentException(string malformedUrl)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WebUrl(malformedUrl));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("http://:8080")]
    [InlineData("https://")]
    [InlineData("http:///")]
    [InlineData("http:///foo")]
    public void Constructor_WithMissingOrEmptyHost_ThrowsArgumentException(string missingHostUrl)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WebUrl(missingHostUrl));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("http://user:pass@example.com")]
    [InlineData("https://user:password@github.com/repo")]
    [InlineData("http://admin@example.com/dashboard")]
    public void Constructor_WithEmbeddedCredentials_ThrowsArgumentException(string credentialUrl)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WebUrl(credentialUrl));
        Assert.Equal("value", exception.ParamName);
        Assert.Contains("credentials", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("JAVASCRIPT:void(0)")]
    [InlineData("data:text/html,<h1>test</h1>")]
    [InlineData("file:///path/to/file.txt")]
    [InlineData("ftp://ftp.example.com/files")]
    [InlineData("mailto:developer@example.com")]
    [InlineData("ws://example.com/socket")]
    [InlineData("wss://example.com/socket")]
    [InlineData("gopher://example.com")]
    public void Constructor_WithDangerousOrUnsupportedSchemes_ThrowsArgumentException(string unsupportedSchemeUrl)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WebUrl(unsupportedSchemeUrl));
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_NormalizesSchemeAndHostToLowercase_AndPreservesPathCasing()
    {
        // Arrange
        var rawUrl = "HTTPS://GITHUB.COM/DotNet/AspNetCore";

        // Act
        var webUrl = new WebUrl(rawUrl);

        // Assert
        Assert.Equal("https://github.com/DotNet/AspNetCore", webUrl.Value);
    }

    [Fact]
    public void Constructor_NormalizesRootUrl_WithTrailingSlash()
    {
        // Arrange
        var rawUrl = "https://example.com";

        // Act
        var webUrl = new WebUrl(rawUrl);

        // Assert
        Assert.Equal("https://example.com/", webUrl.Value);
    }

    [Fact]
    public void Equality_WithIdenticalValues_ReturnsTrue()
    {
        // Arrange
        var url1 = new WebUrl("https://example.com/repo");
        var url2 = new WebUrl("https://example.com/repo");

        // Act & Assert
        Assert.True(url1 == url2);
        Assert.False(url1 != url2);
        Assert.True(url1.Equals(url2));
        Assert.Equal(url1.GetHashCode(), url2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var url1 = new WebUrl("https://example.com/repo-a");
        var url2 = new WebUrl("https://example.com/repo-b");

        // Act & Assert
        Assert.False(url1 == url2);
        Assert.True(url1 != url2);
        Assert.False(url1.Equals(url2));
    }

    [Fact]
    public void Equality_WithDifferentlyCasedHostOrSchemeThatNormalizeIdentically_ReturnsTrue()
    {
        // Arrange
        var url1 = new WebUrl("https://example.com/");
        var url2 = new WebUrl("HTTPS://EXAMPLE.COM");

        // Act & Assert
        Assert.Equal(url1, url2);
        Assert.True(url1 == url2);
    }

    [Fact]
    public void Equality_WithNull_ReturnsFalse()
    {
        // Arrange
        var url = new WebUrl("https://example.com");

        // Act & Assert
        Assert.False(url.Equals(null));
        Assert.False(url == null);
        Assert.False(null == url);
        Assert.True(url != null);
    }

    [Fact]
    public void ToString_ReturnsNormalizedValue()
    {
        // Arrange
        var rawUrl = "https://example.com/showcase";
        var webUrl = new WebUrl(rawUrl);

        // Act & Assert
        Assert.Equal("https://example.com/showcase", webUrl.ToString());
    }

    [Fact]
    public void WebUrl_DoesNotDefineImplicitOrExplicitStringConversionOperators()
    {
        // Act
        var conversionMethods = typeof(WebUrl)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name is "op_Implicit" or "op_Explicit")
            .ToList();

        // Assert
        Assert.Empty(conversionMethods);
    }
}
