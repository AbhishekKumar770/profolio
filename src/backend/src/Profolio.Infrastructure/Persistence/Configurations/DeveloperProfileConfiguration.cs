namespace Profolio.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;

public sealed class DeveloperProfileConfiguration : IEntityTypeConfiguration<DeveloperProfile>
{
    public void Configure(EntityTypeBuilder<DeveloperProfile> builder)
    {
        builder.ToTable("developer_profiles", "profile");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
               .ValueGeneratedNever();

        builder.Property(p => p.CreatedAtUtc)
               .IsRequired();

        builder.Property(p => p.UpdatedAtUtc);

        builder.Property(p => p.TenantId)
               .IsRequired()
               .IsConcurrencyToken();

        builder.Property(p => p.FullName)
               .HasMaxLength(DeveloperProfile.MaxFullNameLength)
               .IsRequired();

        builder.Property(p => p.Headline)
               .HasMaxLength(DeveloperProfile.MaxHeadlineLength)
               .IsRequired();

        builder.Property(p => p.Bio);

        builder.Property(p => p.Location)
               .HasMaxLength(DeveloperProfile.MaxLocationLength);

        builder.Property(p => p.PublicContactEmail)
               .HasConversion(
                   e => e == null ? null : e.Value,
                   v => v == null ? null : new EmailAddress(v))
               .HasMaxLength(254);

        builder.Property(p => p.ProfilePhotoMediaId);

        builder.Property<uint>("xmin")
               .HasColumnType("xid")
               .IsRowVersion();

        builder.HasOne<Tenant>()
               .WithMany()
               .HasForeignKey(p => p.TenantId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.TenantId)
               .IsUnique()
               .HasDatabaseName("ix_developer_profiles_tenant_id");
    }
}
