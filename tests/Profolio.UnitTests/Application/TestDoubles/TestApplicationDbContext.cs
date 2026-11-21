namespace Profolio.UnitTests.Application.TestDoubles;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Profolio.Application.Abstractions.Persistence;
using Profolio.Domain.Entities;

public sealed class TestApplicationDbContext : IApplicationDbContext
{
    public TestDbSet<Tenant> Tenants { get; set; } = new();
    public TestDbSet<DeveloperProfile> DeveloperProfiles { get; set; } = new();
    public TestDbSet<Skill> Skills { get; set; } = new();
    public TestDbSet<WorkExperience> WorkExperiences { get; set; } = new();
    public TestDbSet<Education> Educations { get; set; } = new();
    public TestDbSet<Project> Projects { get; set; } = new();
    public TestDbSet<SocialLink> SocialLinks { get; set; } = new();
    public TestDbSet<PortfolioConfiguration> PortfolioConfigurations { get; set; } = new();
    public TestDbSet<PortfolioVersion> PortfolioVersions { get; set; } = new();
    public TestDbSet<PublishingState> PublishingStates { get; set; } = new();

    DbSet<Tenant> IApplicationDbContext.Tenants => Tenants;
    DbSet<DeveloperProfile> IApplicationDbContext.DeveloperProfiles => DeveloperProfiles;
    DbSet<Skill> IApplicationDbContext.Skills => Skills;
    DbSet<WorkExperience> IApplicationDbContext.WorkExperiences => WorkExperiences;
    DbSet<Education> IApplicationDbContext.Educations => Educations;
    DbSet<Project> IApplicationDbContext.Projects => Projects;
    DbSet<SocialLink> IApplicationDbContext.SocialLinks => SocialLinks;
    DbSet<PortfolioConfiguration> IApplicationDbContext.PortfolioConfigurations => PortfolioConfigurations;
    DbSet<PortfolioVersion> IApplicationDbContext.PortfolioVersions => PortfolioVersions;
    DbSet<PublishingState> IApplicationDbContext.PublishingStates => PublishingStates;

    public int SaveChangesCalls { get; private set; }
    public Func<CancellationToken, Task<int>>? SaveChangesBehavior { get; set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SaveChangesCalls++;

        if (SaveChangesBehavior != null)
        {
            return SaveChangesBehavior(cancellationToken);
        }

        return Task.FromResult(1);
    }
}
