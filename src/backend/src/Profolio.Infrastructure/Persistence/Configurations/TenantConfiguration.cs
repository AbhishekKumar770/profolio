namespace Profolio.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profolio.Domain.Common;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants", "tenancy");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
               .ValueGeneratedNever();

        builder.Property(t => t.CreatedAtUtc)
               .IsRequired();

        builder.Property(t => t.UpdatedAtUtc);

        builder.Property(t => t.OwnerUserId)
               .IsRequired();

        builder.Property(t => t.Username)
               .HasConversion(
                   u => u.Value,
                   v => new Username(v))
               .HasMaxLength(Username.MaxLength)
               .IsRequired();

        builder.Ignore(nameof(ITenantEntity.TenantId));

        builder.HasIndex(t => t.OwnerUserId)
               .IsUnique()
               .HasDatabaseName("ix_tenants_owner_user_id");

        builder.HasIndex(t => t.Username)
               .IsUnique()
               .HasDatabaseName("ix_tenants_username");
    }
}
