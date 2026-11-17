using System.Collections;
using Profolio.Domain.ValueObjects;

namespace Profolio.UnitTests;

public class PortfolioSectionTypeTests
{
    [Theory]
    [InlineData("about", "about")]
    [InlineData("skills", "skills")]
    [InlineData("experience", "experience")]
    [InlineData("education", "education")]
    [InlineData("projects", "projects")]
    [InlineData("social-links", "social-links")]
    [InlineData("contact", "contact")]
    public void Constructor_WithSupportedSection_CreatesPortfolioSectionType(string input, string expectedCanonical)
    {
        // Act
        var section = new PortfolioSectionType(input);

        // Assert
        Assert.Equal(expectedCanonical, section.Value);
        Assert.Equal(expectedCanonical, section.ToString());
    }

    [Theory]
    [InlineData("About", "about")]
    [InlineData("ABOUT", "about")]
    [InlineData("Skills", "skills")]
    [InlineData("SKILLS", "skills")]
    [InlineData("Experience", "experience")]
    [InlineData("EXPERIENCE", "experience")]
    [InlineData("Education", "education")]
    [InlineData("EDUCATION", "education")]
    [InlineData("Projects", "projects")]
    [InlineData("PROJECTS", "projects")]
    [InlineData("Social-Links", "social-links")]
    [InlineData("SOCIAL-LINKS", "social-links")]
    [InlineData("Contact", "contact")]
    [InlineData("CONTACT", "contact")]
    public void Constructor_WithCaseInsensitiveKnownSection_NormalizesToLowercase(string input, string expectedCanonical)
    {
        // Act
        var section = new PortfolioSectionType(input);

        // Assert
        Assert.Equal(expectedCanonical, section.Value);
    }

    [Theory]
    [InlineData("   about   ", "about")]
    [InlineData("  skills  ", "skills")]
    [InlineData(" social-links ", "social-links")]
    public void Constructor_WithUntrimmedInput_TrimsWhitespace(string input, string expectedCanonical)
    {
        // Act
        var section = new PortfolioSectionType(input);

        // Assert
        Assert.Equal(expectedCanonical, section.Value);
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new PortfolioSectionType(null!));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespace_ThrowsArgumentException(string invalidInput)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PortfolioSectionType(invalidInput));
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithLengthExceedingMaximum_ThrowsArgumentException()
    {
        // Arrange
        var excessiveString = new string('s', PortfolioSectionType.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PortfolioSectionType(excessiveString));
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("blog")]
    [InlineData("pricing")]
    [InlineData("testimonials")]
    [InlineData("custom")]
    [InlineData("sociallinks")] // Undashed alias not supported by strict MVP catalog
    [InlineData("social_links")]
    public void Constructor_WithUnsupportedSection_ThrowsArgumentException(string unsupported)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PortfolioSectionType(unsupported));
        Assert.Equal("value", exception.ParamName);
        Assert.Contains("Unsupported portfolio section type", exception.Message);
    }

    [Fact]
    public void StaticInstances_HaveExpectedCanonicalValues()
    {
        // Assert
        Assert.Equal("about", PortfolioSectionType.About.Value);
        Assert.Equal("skills", PortfolioSectionType.Skills.Value);
        Assert.Equal("experience", PortfolioSectionType.Experience.Value);
        Assert.Equal("education", PortfolioSectionType.Education.Value);
        Assert.Equal("projects", PortfolioSectionType.Projects.Value);
        Assert.Equal("social-links", PortfolioSectionType.SocialLinks.Value);
        Assert.Equal("contact", PortfolioSectionType.Contact.Value);
    }

    [Fact]
    public void All_ContainsExactlySevenUniqueCatalogSections()
    {
        // Assert
        Assert.Equal(7, PortfolioSectionType.All.Count);
        Assert.Equal(7, PortfolioSectionType.All.Distinct().Count());

        Assert.Contains(PortfolioSectionType.About, PortfolioSectionType.All);
        Assert.Contains(PortfolioSectionType.Skills, PortfolioSectionType.All);
        Assert.Contains(PortfolioSectionType.Experience, PortfolioSectionType.All);
        Assert.Contains(PortfolioSectionType.Education, PortfolioSectionType.All);
        Assert.Contains(PortfolioSectionType.Projects, PortfolioSectionType.All);
        Assert.Contains(PortfolioSectionType.SocialLinks, PortfolioSectionType.All);
        Assert.Contains(PortfolioSectionType.Contact, PortfolioSectionType.All);
    }

    [Fact]
    public void All_IsImmutable_CannotBeMutatedByCallers()
    {
        // Arrange
        var catalog = PortfolioSectionType.All;

        // Act & Assert
        Assert.IsAssignableFrom<IReadOnlyList<PortfolioSectionType>>(catalog);
        if (catalog is IList mutableList)
        {
            Assert.Throws<NotSupportedException>(() => mutableList.Add(new PortfolioSectionType("about")));
            Assert.Throws<NotSupportedException>(() => mutableList.Clear());
        }
    }

    [Fact]
    public void Equality_WithSameCanonicalValue_EvaluatesAsEqual()
    {
        // Arrange
        var fromString = new PortfolioSectionType("ABOUT");
        var fromStatic = PortfolioSectionType.About;

        // Assert
        Assert.Equal(fromStatic, fromString);
        Assert.True(fromStatic == fromString);
        Assert.False(fromStatic != fromString);
        Assert.Equal(fromStatic.GetHashCode(), fromString.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentSectionType_EvaluatesAsNotEqual()
    {
        // Arrange
        var about = PortfolioSectionType.About;
        var skills = PortfolioSectionType.Skills;

        // Assert
        Assert.NotEqual(about, skills);
        Assert.True(about != skills);
        Assert.False(about == skills);
    }
}
