using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profile.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Infrastructure.Configurations;

public sealed class ExternalProfileTypeConfiguration : IEntityTypeConfiguration<ExternalProfileType>
{
    public void Configure(EntityTypeBuilder<ExternalProfileType> builder)
    {
        builder.ToTable("external_profile_types");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CountryCode).IsUnique();

        builder.Property(x => x.CountryName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();
        builder.Property(x => x.PhoneIndicatif).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(255).IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.UpdatedBy).HasMaxLength(255);
    }
}
