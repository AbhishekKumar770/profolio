namespace Profolio.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;

public sealed class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    public void Configure(EntityTypeBuilder<SocialLink> builder)
    {
        builder.ToTable("social_links", "profile");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
               .ValueGeneratedNever();

        builder.Property(s => s.CreatedAtUtc)
               .IsRequired();

        builder.Property(s => s.UpdatedAtUtc);

        builder.Property(s => s.TenantId)
               .IsRequired()
               .IsConcurrencyToken();

        builder.Property(s => s.Platform)
               .HasConversion(
                   p => p.Value,
                   v => new SocialPlatform(v))
               .HasMaxLength(SocialPlatform.MaxLength)
               .IsRequired();

        builder.Property(s => s.Url)
               .HasConversion(
                   u => u.Value,
                   v => new WebUrl(v))
               .HasMaxLength(WebUrl.MaxLength)
               .IsRequired();

        builder.Property(s => s.Label)
               .HasMaxLength(SocialLink.MaxLabelLength);

        builder.Property(s => s.DisplayOrder)
               .IsRequired();

        builder.Property<uint>("xmin")
               .HasColumnType("xid")
               .IsRowVersion();

        builder.HasOne<Tenant>()
               .WithMany()
               .HasForeignKey(s => s.TenantId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.TenantId, s.DisplayOrder })
               .HasDatabaseName("ix_social_links_tenant_id_display_order");
    }
}
