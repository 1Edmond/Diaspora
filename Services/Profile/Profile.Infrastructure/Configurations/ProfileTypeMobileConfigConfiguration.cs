using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profile.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Profile.Infrastructure.Configurations;

public sealed class ProfileTypeMobileConfigConfiguration : IEntityTypeConfiguration<ProfileTypeMobileConfig>
{
    public void Configure(EntityTypeBuilder<ProfileTypeMobileConfig> builder)
    {
        builder.ToTable("mobile_configs");

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ProfileTypeId).IsUnique();

        builder.Property(x => x.CountryCode).HasMaxLength(10);
        builder.Property(x => x.MinAppVersion).HasMaxLength(20).IsRequired().HasDefaultValue("1.0.0");
        builder.Property(x => x.ForceUpdate).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsMaintenanceMode).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.MaintenanceMessage).HasMaxLength(500);
        builder.Property(x => x.DefaultLocale).HasMaxLength(10).IsRequired().HasDefaultValue("fr");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(255);

        builder.Property(x => x.ActiveModules)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, bool>>(v, (JsonSerializerOptions?)null)
                    ?? new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            )
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.SupportedLocales)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                    ?? new List<string>()
            )
            .HasColumnType("text")
            .IsRequired();
    }
}
