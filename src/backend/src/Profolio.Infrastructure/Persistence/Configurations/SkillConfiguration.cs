namespace Profolio.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profolio.Domain.Entities;

public sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("skills", "portfolio");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
               .ValueGeneratedNever();

        builder.Property(s => s.CreatedAtUtc)
               .IsRequired();

        builder.Property(s => s.UpdatedAtUtc);

        builder.Property(s => s.TenantId)
               .IsRequired()
               .IsConcurrencyToken();

        builder.Property(s => s.Name)
               .HasMaxLength(Skill.MaxNameLength)
               .IsRequired();

        builder.Property(s => s.Category)
               .HasMaxLength(Skill.MaxCategoryLength);

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
               .HasDatabaseName("ix_skills_tenant_id_display_order");
    }
}
