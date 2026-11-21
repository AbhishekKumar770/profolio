namespace Profolio.UnitTests.Application.Features.Skills;

using Microsoft.Extensions.DependencyInjection;
using Profolio.Application;
using Profolio.Application.Abstractions.Persistence;
using Profolio.Application.Abstractions.Tenancy;
using Profolio.Application.Common.Handlers;
using Profolio.Application.Features.Skills;
using Profolio.Application.Features.Skills.Commands.AddSkill;
using Profolio.Application.Features.Skills.Commands.DeleteSkill;
using Profolio.Application.Features.Skills.Commands.ReorderSkills;
using Profolio.Application.Features.Skills.Commands.UpdateSkill;
using Profolio.Application.Features.Skills.Queries.GetSkills;
using Profolio.UnitTests.Application.TestDoubles;
using Profolio.UnitTests.Infrastructure.Persistence;

public sealed class SkillsDependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersAllSkillsHandlersAsScoped()
    {
        var services = new ServiceCollection();
        services.AddScoped<IApplicationDbContext, TestApplicationDbContext>();
        services.AddScoped<ITenantContext>(_ => TestTenantContext.AuthenticatedWithTenant(Guid.NewGuid()));
        services.AddApplication();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        AssertScoped<IQueryHandler<GetSkillsQuery, IReadOnlyList<SkillDto>>, GetSkillsQueryHandler>(services, scope.ServiceProvider);
        AssertScoped<ICommandHandler<AddSkillCommand, Guid>, AddSkillCommandHandler>(services, scope.ServiceProvider);
        AssertScoped<ICommandHandler<UpdateSkillCommand>, UpdateSkillCommandHandler>(services, scope.ServiceProvider);
        AssertScoped<ICommandHandler<DeleteSkillCommand>, DeleteSkillCommandHandler>(services, scope.ServiceProvider);
        AssertScoped<ICommandHandler<ReorderSkillsCommand>, ReorderSkillsCommandHandler>(services, scope.ServiceProvider);
    }

    private static void AssertScoped<TService, TImplementation>(IServiceCollection services, IServiceProvider provider)
        where TService : class where TImplementation : class
    {
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(TService) &&
            descriptor.ImplementationType == typeof(TImplementation) && descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.IsType<TImplementation>(provider.GetRequiredService<TService>());
    }
}
