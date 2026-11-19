namespace Profolio.Infrastructure.Persistence.Configurations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;

public sealed class PortfolioConfigurationConfiguration : IEntityTypeConfiguration<PortfolioConfiguration>
{
    private sealed record SectionDto(string Type, bool IsEnabled, int DisplayOrder);

    public void Configure(EntityTypeBuilder<PortfolioConfiguration> builder)
    {
        builder.ToTable("portfolio_configurations", "website");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
               .ValueGeneratedNever();

        builder.Property(c => c.CreatedAtUtc)
               .IsRequired();

        builder.Property(c => c.UpdatedAtUtc);

        builder.Property(c => c.TenantId)
               .IsRequired()
               .IsConcurrencyToken();

        builder.Property(c => c.Template)
               .HasConversion(
                   t => t.Value,
                   v => new PortfolioTemplate(v))
               .HasMaxLength(PortfolioTemplate.MaxLength)
               .IsRequired();

        var sectionsConverter = new ValueConverter<List<PortfolioSectionConfig>, string>(
            list => JsonSerializer.Serialize(
                list.Select(s => new SectionDto(s.SectionType.Value, s.IsEnabled, s.DisplayOrder)),
                (JsonSerializerOptions?)null),
            json => (JsonSerializer.Deserialize<List<SectionDto>>(json, (JsonSerializerOptions?)null) ?? new List<SectionDto>())
                .Select(dto => new PortfolioSectionConfig(new PortfolioSectionType(dto.Type), dto.IsEnabled, dto.DisplayOrder))
                .ToList());

        var sectionsComparer = new ValueComparer<List<PortfolioSectionConfig>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.Select(s => new PortfolioSectionConfig(s.SectionType, s.IsEnabled, s.DisplayOrder)).ToList());

        builder.Ignore(c => c.Sections);

        builder.Property<List<PortfolioSectionConfig>>("_sections")
               .HasColumnName("sections")
               .HasColumnType("jsonb")
               .UsePropertyAccessMode(PropertyAccessMode.Field)
               .HasConversion(sectionsConverter, sectionsComparer)
               .IsRequired();

        builder.Property<uint>("xmin")
               .HasColumnType("xid")
               .IsRowVersion();

        builder.HasOne<Tenant>()
               .WithMany()
               .HasForeignKey(c => c.TenantId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.TenantId)
               .IsUnique()
               .HasDatabaseName("ix_portfolio_configurations_tenant_id");
    }
}
