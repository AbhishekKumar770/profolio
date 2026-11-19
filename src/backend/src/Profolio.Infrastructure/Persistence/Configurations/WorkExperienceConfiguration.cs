namespace Profolio.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profolio.Domain.Entities;

public sealed class WorkExperienceConfiguration : IEntityTypeConfiguration<WorkExperience>
{
    public void Configure(EntityTypeBuilder<WorkExperience> builder)
    {
        builder.ToTable("work_experiences", "portfolio");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
               .ValueGeneratedNever();

        builder.Property(w => w.CreatedAtUtc)
               .IsRequired();

        builder.Property(w => w.UpdatedAtUtc);

        builder.Property(w => w.TenantId)
               .IsRequired()
               .IsConcurrencyToken();

        builder.Property(w => w.CompanyName)
               .HasMaxLength(WorkExperience.MaxCompanyNameLength)
               .IsRequired();

        builder.Property(w => w.Role)
               .HasMaxLength(WorkExperience.MaxRoleLength)
               .IsRequired();

        builder.Property(w => w.Location)
               .HasMaxLength(WorkExperience.MaxLocationLength);

        builder.Property(w => w.StartDate)
               .IsRequired();

        builder.Property(w => w.EndDate);

        builder.Property(w => w.IsCurrent)
               .IsRequired();

        builder.Property(w => w.Description)
               .HasMaxLength(WorkExperience.MaxDescriptionLength);

        builder.Property(w => w.DisplayOrder)
               .IsRequired();

        builder.Property<uint>("xmin")
               .HasColumnType("xid")
               .IsRowVersion();

        builder.HasOne<Tenant>()
               .WithMany()
               .HasForeignKey(w => w.TenantId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => new { w.TenantId, w.DisplayOrder })
               .HasDatabaseName("ix_work_experiences_tenant_id_display_order");
    }
}
