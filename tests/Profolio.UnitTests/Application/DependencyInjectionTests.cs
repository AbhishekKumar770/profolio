namespace Profolio.UnitTests.Application;

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Profolio.Application;
using Profolio.Application.Abstractions.Persistence;
using Profolio.Application.Abstractions.Tenancy;
using Profolio.Application.Common.Handlers;
using Profolio.Application.Features.DeveloperProfile.Commands.ClearPublicContactEmail;
using Profolio.Application.Features.DeveloperProfile.Commands.SetPublicContactEmail;
using Profolio.Application.Features.DeveloperProfile.Commands.UpdateDeveloperProfile;
using Profolio.Application.Features.DeveloperProfile.Queries.GetDeveloperProfile;
using Profolio.UnitTests.Application.TestDoubles;
using Profolio.UnitTests.Infrastructure.Persistence;
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

    [Theory]
    [InlineData(typeof(IQueryHandler<GetDeveloperProfileQuery, DeveloperProfileDto>), typeof(GetDeveloperProfileQueryHandler))]
    [InlineData(typeof(ICommandHandler<UpdateDeveloperProfileCommand>), typeof(UpdateDeveloperProfileCommandHandler))]
    [InlineData(typeof(ICommandHandler<SetPublicContactEmailCommand>), typeof(SetPublicContactEmailCommandHandler))]
    [InlineData(typeof(ICommandHandler<ClearPublicContactEmailCommand>), typeof(ClearPublicContactEmailCommandHandler))]
    public void AddApplication_RegistersExpectedDeveloperProfileHandlers(Type serviceType, Type implementationType)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IApplicationDbContext>(_ => new TestApplicationDbContext());
        services.AddScoped<ITenantContext>(_ => TestTenantContext.Anonymous());

        // Act
        services.AddApplication();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Assert
        var resolved = scope.ServiceProvider.GetRequiredService(serviceType);
        Assert.NotNull(resolved);
        Assert.IsType(implementationType, resolved);
    }

    [Theory]
    [InlineData(typeof(IQueryHandler<GetDeveloperProfileQuery, DeveloperProfileDto>), ServiceLifetime.Scoped)]
    [InlineData(typeof(ICommandHandler<UpdateDeveloperProfileCommand>), ServiceLifetime.Scoped)]
    [InlineData(typeof(ICommandHandler<SetPublicContactEmailCommand>), ServiceLifetime.Scoped)]
    [InlineData(typeof(ICommandHandler<ClearPublicContactEmailCommand>), ServiceLifetime.Scoped)]
    public void AddApplication_RegistersHandlersWithScopedLifetime(Type serviceType, ServiceLifetime expectedLifetime)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == serviceType);
        Assert.NotNull(descriptor);
        Assert.Equal(expectedLifetime, descriptor.Lifetime);
    }
}
