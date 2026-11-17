using Profolio.Domain.ValueObjects;

namespace Profolio.UnitTests;

public class PortfolioSectionConfigTests
{
    [Fact]
    public void Constructor_WithValidArguments_InitializesProperties()
    {
        // Arrange
        var sectionType = PortfolioSectionType.About;
        var isEnabled = true;
        var displayOrder = 0;

        // Act
        var config = new PortfolioSectionConfig(sectionType, isEnabled, displayOrder);

        // Assert
        Assert.Equal(sectionType, config.SectionType);
        Assert.True(config.IsEnabled);
        Assert.Equal(0, config.DisplayOrder);
    }

    [Fact]
    public void Constructor_WithNullSectionType_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new PortfolioSectionConfig(null!, true, 0));
        Assert.Equal("sectionType", exception.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_WithNegativeDisplayOrder_ThrowsArgumentOutOfRangeException(int negativeDisplayOrder)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PortfolioSectionConfig(PortfolioSectionType.Skills, true, negativeDisplayOrder));
        Assert.Equal("displayOrder", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithZeroDisplayOrder_Succeeds()
    {
        // Act
        var config = new PortfolioSectionConfig(PortfolioSectionType.Experience, false, 0);

        // Assert
        Assert.Equal(0, config.DisplayOrder);
        Assert.False(config.IsEnabled);
    }

    [Fact]
    public void Equality_WithIdenticalValues_EvaluatesAsEqual()
    {
        // Arrange
        var config1 = new PortfolioSectionConfig(PortfolioSectionType.Projects, true, 4);
        var config2 = new PortfolioSectionConfig(new PortfolioSectionType("projects"), true, 4);

        // Assert
        Assert.Equal(config1, config2);
        Assert.True(config1 == config2);
        Assert.False(config1 != config2);
        Assert.Equal(config1.GetHashCode(), config2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentIsEnabled_EvaluatesAsNotEqual()
    {
        // Arrange
        var config1 = new PortfolioSectionConfig(PortfolioSectionType.Projects, true, 4);
        var config2 = new PortfolioSectionConfig(PortfolioSectionType.Projects, false, 4);

        // Assert
        Assert.NotEqual(config1, config2);
        Assert.True(config1 != config2);
    }

    [Fact]
    public void Equality_WithDifferentDisplayOrder_EvaluatesAsNotEqual()
    {
        // Arrange
        var config1 = new PortfolioSectionConfig(PortfolioSectionType.Projects, true, 4);
        var config2 = new PortfolioSectionConfig(PortfolioSectionType.Projects, true, 5);

        // Assert
        Assert.NotEqual(config1, config2);
        Assert.True(config1 != config2);
    }

    [Fact]
    public void Equality_WithDifferentSectionType_EvaluatesAsNotEqual()
    {
        // Arrange
        var config1 = new PortfolioSectionConfig(PortfolioSectionType.Projects, true, 4);
        var config2 = new PortfolioSectionConfig(PortfolioSectionType.Education, true, 4);

        // Assert
        Assert.NotEqual(config1, config2);
        Assert.True(config1 != config2);
    }
}
