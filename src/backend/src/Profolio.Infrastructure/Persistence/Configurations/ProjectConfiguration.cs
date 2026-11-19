namespace Profolio.Infrastructure.Persistence.Configurations;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects", "portfolio");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
               .ValueGeneratedNever();

        builder.Property(p => p.CreatedAtUtc)
               .IsRequired();

        builder.Property(p => p.UpdatedAtUtc);

        builder.Property(p => p.TenantId)
               .IsRequired()
               .IsConcurrencyToken();

        builder.Property(p => p.Title)
               .HasMaxLength(Project.MaxTitleLength)
               .IsRequired();

        builder.Property(p => p.ShortDescription)
               .HasMaxLength(Project.MaxShortDescriptionLength);

        builder.Property(p => p.Description)
               .HasMaxLength(Project.MaxDescriptionLength);

        builder.Property(p => p.GitHubUrl)
               .HasConversion(
                   u => u == null ? null : u.Value,
                   v => v == null ? null : new WebUrl(v))
               .HasMaxLength(WebUrl.MaxLength);

        builder.Property(p => p.LiveUrl)
               .HasConversion(
                   u => u == null ? null : u.Value,
                   v => v == null ? null : new WebUrl(v))
               .HasMaxLength(WebUrl.MaxLength);

        builder.Property(p => p.IsFeatured)
               .IsRequired();

        builder.Property(p => p.DisplayOrder)
               .IsRequired();

        builder.Ignore(p => p.Technologies);

        var technologiesComparer = new ValueComparer<List<string>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property<List<string>>("_technologies")
               .HasColumnName("technologies")
               .HasColumnType("text[]")
               .UsePropertyAccessMode(PropertyAccessMode.Field)
               .HasConversion(
                   list => list.ToArray(),
                   arr => arr.ToList(),
                   technologiesComparer)
               .IsRequired();

        builder.Property<uint>("xmin")
               .HasColumnType("xid")
               .IsRowVersion();

        builder.HasOne<Tenant>()
               .WithMany()
               .HasForeignKey(p => p.TenantId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.TenantId, p.DisplayOrder })
               .HasDatabaseName("ix_projects_tenant_id_display_order");

        builder.HasIndex(p => new { p.TenantId, p.IsFeatured })
               .HasDatabaseName("ix_projects_tenant_id_is_featured");
    }
}
