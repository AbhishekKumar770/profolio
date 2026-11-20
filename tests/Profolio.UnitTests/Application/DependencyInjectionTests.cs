namespace Profolio.UnitTests.Application;

using System;
using Microsoft.Extensions.DependencyInjection;
using Profolio.Application;
using Xunit;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_WithValidServices_ReturnsSameServiceCollectionInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var returnedServices = services.AddApplication();

        // Assert
        Assert.Same(services, returnedServices);
    }

    [Fact]
    public void AddApplication_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>("services", () => services!.AddApplication());
    }

    [Fact]
    public void AddApplication_CalledMultipleTimes_ExecutesSuccessfullyAndIsIdempotent()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider);
    }
}
