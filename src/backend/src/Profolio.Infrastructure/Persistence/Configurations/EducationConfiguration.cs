namespace Profolio.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profolio.Domain.Entities;

public sealed class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> builder)
    {
        builder.ToTable("educations", "portfolio");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
               .ValueGeneratedNever();

        builder.Property(e => e.CreatedAtUtc)
               .IsRequired();

        builder.Property(e => e.UpdatedAtUtc);

        builder.Property(e => e.TenantId)
               .IsRequired()
               .IsConcurrencyToken();

        builder.Property(e => e.Institution)
               .HasMaxLength(Education.MaxInstitutionLength)
               .IsRequired();

        builder.Property(e => e.Degree)
               .HasMaxLength(Education.MaxDegreeLength);

        builder.Property(e => e.FieldOfStudy)
               .HasMaxLength(Education.MaxFieldOfStudyLength);

        builder.Property(e => e.Location)
               .HasMaxLength(Education.MaxLocationLength);

        builder.Property(e => e.StartDate)
               .IsRequired();

        builder.Property(e => e.EndDate);

        builder.Property(e => e.IsCurrent)
               .IsRequired();

        builder.Property(e => e.Description)
               .HasMaxLength(Education.MaxDescriptionLength);

        builder.Property(e => e.DisplayOrder)
               .IsRequired();

        builder.Property<uint>("xmin")
               .HasColumnType("xid")
               .IsRowVersion();

        builder.HasOne<Tenant>()
               .WithMany()
               .HasForeignKey(e => e.TenantId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TenantId, e.DisplayOrder })
               .HasDatabaseName("ix_educations_tenant_id_display_order");
    }
}
