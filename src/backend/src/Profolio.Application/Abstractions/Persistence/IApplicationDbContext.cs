using Microsoft.EntityFrameworkCore;
using Profolio.Domain.Entities;

namespace Profolio.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }

    DbSet<DeveloperProfile> DeveloperProfiles { get; }

    DbSet<Skill> Skills { get; }

    DbSet<WorkExperience> WorkExperiences { get; }

    DbSet<Education> Educations { get; }

    DbSet<Project> Projects { get; }

    DbSet<SocialLink> SocialLinks { get; }

    DbSet<PortfolioConfiguration> PortfolioConfigurations { get; }

    DbSet<PortfolioVersion> PortfolioVersions { get; }

    DbSet<PublishingState> PublishingStates { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
