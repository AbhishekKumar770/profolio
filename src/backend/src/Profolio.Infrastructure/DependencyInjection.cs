namespace Profolio.Infrastructure;

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Profolio.Application.Abstractions.Persistence;
using Profolio.Infrastructure.Persistence;
using Profolio.Infrastructure.Persistence.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructurePersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration.GetConnectionString("ProfolioDb")
            ?? throw new InvalidOperationException("PostgreSQL connection string 'DefaultConnection' or 'ProfolioDb' was not found.");

        services.AddDbContext<ProfolioDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ProfolioDbContext).Assembly.FullName);
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "public");
            });
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ProfolioDbContext>());
        services.AddScoped<IPublicPortfolioReader, PublicPortfolioReader>();
        services.AddScoped<ITenantDirectory, TenantDirectory>();

        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
