using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profile.Domain.Entities;
using Shared.Messaging.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Infrastructure.Configurations;

public sealed class InternalProfileTypeConfiguration : IEntityTypeConfiguration<ProfileType>
{
    public void Configure(EntityTypeBuilder<ProfileType> builder)
    {
        builder.ToTable("profile_types");

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

        builder.HasData(new
        {
            Id = ServiceDataConstants.TgInternalProfileTypeId,
            CountryName = "Togo",
            CountryCode = "TG",
            PhoneIndicatif = "+228",
            Currency = "XOF",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "System",
            UpdatedAt = (DateTime?)null,
            UpdatedBy = (string?)null
        });
    }
}
