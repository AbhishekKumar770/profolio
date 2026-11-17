using Profolio.Domain.ValueObjects;

namespace Profolio.UnitTests;

public class PortfolioTemplateTests
{
    [Theory]
    [InlineData("modern", "modern")]
    [InlineData("minimal", "minimal")]
    public void Constructor_WithSupportedTemplate_CreatesPortfolioTemplate(string input, string expectedCanonical)
    {
        // Act
        var template = new PortfolioTemplate(input);

        // Assert
        Assert.Equal(expectedCanonical, template.Value);
        Assert.Equal(expectedCanonical, template.ToString());
    }

    [Theory]
    [InlineData("Modern", "modern")]
    [InlineData("MODERN", "modern")]
    [InlineData("mOdErN", "modern")]
    [InlineData("Minimal", "minimal")]
    [InlineData("MINIMAL", "minimal")]
    [InlineData("mInImAl", "minimal")]
    public void Constructor_WithCaseInsensitiveTemplate_NormalizesToLowercase(string input, string expectedCanonical)
    {
        // Act
        var template = new PortfolioTemplate(input);

        // Assert
        Assert.Equal(expectedCanonical, template.Value);
    }

    [Theory]
    [InlineData("   modern   ", "modern")]
    [InlineData("  MODERN  ", "modern")]
    [InlineData(" minimal ", "minimal")]
    [InlineData("  MINIMAL  ", "minimal")]
    public void Constructor_WithUntrimmedInput_TrimsWhitespace(string input, string expectedCanonical)
    {
        // Act
        var template = new PortfolioTemplate(input);

        // Assert
        Assert.Equal(expectedCanonical, template.Value);
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new PortfolioTemplate(null!));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespace_ThrowsArgumentException(string invalidInput)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PortfolioTemplate(invalidInput));
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithLengthExceedingMaximum_ThrowsArgumentException()
    {
        // Arrange
        var excessiveString = new string('a', PortfolioTemplate.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PortfolioTemplate(excessiveString));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("classic")]
    [InlineData("futuristic")]
    [InlineData("standard")]
    [InlineData("dark")]
    [InlineData("custom")]
    public void Constructor_WithUnsupportedTemplate_ThrowsArgumentException(string unsupported)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PortfolioTemplate(unsupported));
        Assert.Equal("value", exception.ParamName);
        Assert.Contains("Unsupported portfolio template", exception.Message);
    }

    [Fact]
    public void StaticInstances_HaveExpectedCanonicalValues()
    {
        // Assert
        Assert.Equal("modern", PortfolioTemplate.Modern.Value);
        Assert.Equal("minimal", PortfolioTemplate.Minimal.Value);
    }

    [Fact]
    public void Equality_WithSameCanonicalValue_EvaluatesAsEqual()
    {
        // Arrange
        var fromString = new PortfolioTemplate("MODERN");
        var fromStatic = PortfolioTemplate.Modern;

        // Assert
        Assert.Equal(fromStatic, fromString);
        Assert.True(fromStatic == fromString);
        Assert.False(fromStatic != fromString);
        Assert.Equal(fromStatic.GetHashCode(), fromString.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentTemplate_EvaluatesAsNotEqual()
    {
        // Arrange
        var modern = PortfolioTemplate.Modern;
        var minimal = PortfolioTemplate.Minimal;

        // Assert
        Assert.NotEqual(modern, minimal);
        Assert.True(modern != minimal);
        Assert.False(modern == minimal);
    }
}
