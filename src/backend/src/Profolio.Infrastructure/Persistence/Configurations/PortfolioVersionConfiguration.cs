namespace Profolio.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profolio.Domain.Entities;

public sealed class PortfolioVersionConfiguration : IEntityTypeConfiguration<PortfolioVersion>
{
    public void Configure(EntityTypeBuilder<PortfolioVersion> builder)
    {
        builder.ToTable("portfolio_versions", "website");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
               .ValueGeneratedNever();

        builder.Property(v => v.CreatedAtUtc)
               .IsRequired();

        builder.Property(v => v.UpdatedAtUtc);

        builder.Property(v => v.TenantId)
               .IsRequired();

        builder.Property(v => v.VersionNumber)
               .IsRequired();

        builder.Property(v => v.SnapshotJson)
               .HasColumnType("jsonb")
               .IsRequired();

        builder.Property(v => v.SnapshotSchemaVersion)
               .IsRequired();

        builder.HasAlternateKey(v => new { v.TenantId, v.Id })
               .HasName("ak_portfolio_versions_tenant_id_id");

        builder.HasOne<Tenant>()
               .WithMany()
               .HasForeignKey(v => v.TenantId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => new { v.TenantId, v.VersionNumber })
               .IsUnique()
               .HasDatabaseName("ix_portfolio_versions_tenant_id_version_number");

        builder.HasIndex(v => new { v.TenantId, v.CreatedAtUtc })
               .HasDatabaseName("ix_portfolio_versions_tenant_id_created_at_utc");
    }
}
