namespace Profolio.Infrastructure.Persistence;

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Profolio.Application.Abstractions.Tenancy;

public sealed class ProfolioDbContextFactory : IDesignTimeDbContextFactory<ProfolioDbContext>
{
    public ProfolioDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProfolioDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Design-time connection string is not configured. " +
                "Set the environment variable 'ConnectionStrings__DefaultConnection' to execute EF Core design-time commands.");
        }

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(ProfolioDbContext).Assembly.FullName);
            npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "public");
        });

        var designTimeTenantContext = new DesignTimeTenantContext();

        return new ProfolioDbContext(optionsBuilder.Options, designTimeTenantContext);
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
        public string? UserId => null;
        public bool IsAuthenticated => false;
    }
}
