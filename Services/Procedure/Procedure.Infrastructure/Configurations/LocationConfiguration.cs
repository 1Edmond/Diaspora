using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Procedure.Domain.Entities;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Procedure.Infrastructure.Configurations;

public sealed class LocationConfiguration : BaseEntityConfiguration<Location>
{
    public override void Configure(EntityTypeBuilder<Location> builder)
    {
        base.Configure(builder);

        builder.ToTable("locations");

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Latitude).IsRequired();
        builder.Property(x => x.Longitude).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(30);
        builder.Property(x => x.Website).HasMaxLength(255);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        // Address — owned type (colonnes séparées)
        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("address_street")
                .HasMaxLength(255)
                .IsRequired();
            address.Property(a => a.City)
                .HasColumnName("address_city")
                .HasMaxLength(100)
                .IsRequired();
            address.Property(a => a.State)
                .HasColumnName("address_state")
                .HasMaxLength(100);
            address.Property(a => a.PostalCode)
                .HasColumnName("address_postal_code")
                .HasMaxLength(20);
            address.Property(a => a.Country)
                .HasColumnName("address_country")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(x => x.OpeningHours)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<OpeningHours>(v, (JsonSerializerOptions?)null)
                     ?? OpeningHours.Empty());
    }
}
