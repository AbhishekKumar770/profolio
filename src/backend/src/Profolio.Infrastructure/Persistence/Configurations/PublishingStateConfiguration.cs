namespace Profolio.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profolio.Domain.Entities;

public sealed class PublishingStateConfiguration : IEntityTypeConfiguration<PublishingState>
{
    public void Configure(EntityTypeBuilder<PublishingState> builder)
    {
        builder.ToTable("publishing_states", "website");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
               .ValueGeneratedNever();

        builder.Property(p => p.CreatedAtUtc)
               .IsRequired();

        builder.Property(p => p.UpdatedAtUtc);

        builder.Property(p => p.TenantId)
               .IsRequired()
               .IsConcurrencyToken();

        builder.Property(p => p.IsPublished)
               .IsRequired();

        builder.Property(p => p.ActivePortfolioVersionId);

        builder.Property(p => p.ActiveVersionNumber);

        builder.Property(p => p.PublishedAtUtc);

        builder.Property(p => p.UnpublishedAtUtc);

        builder.Property<uint>("xmin")
               .HasColumnType("xid")
               .IsRowVersion();

        builder.HasOne<Tenant>()
               .WithMany()
               .HasForeignKey(p => p.TenantId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PortfolioVersion>()
               .WithMany()
               .HasForeignKey(p => new { p.TenantId, p.ActivePortfolioVersionId })
               .HasPrincipalKey(v => new { v.TenantId, v.Id })
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.TenantId)
               .IsUnique()
               .HasDatabaseName("ix_publishing_states_tenant_id");
    }
}
