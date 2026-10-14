using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Infrastructure.Configurations;

public class ServiceListingConfiguration : BaseEntityConfiguration<ServiceListing>
{
    public override void Configure(EntityTypeBuilder<ServiceListing> builder)
    {
        base.Configure(builder);

        builder.ToTable("service_listings");

        builder.Property(l => l.Title).HasMaxLength(150).IsRequired();
        builder.Property(l => l.Description).HasMaxLength(2000).IsRequired();
        builder.Property(l => l.ContactInfo).HasMaxLength(500);
        builder.Property(l => l.Price).HasPrecision(18, 2);
        builder.Property(l => l.Currency).HasMaxLength(10);
        builder.Property(l => l.AverageRating).HasPrecision(3, 2);

        // Localisation
        builder.Property(l => l.City).HasMaxLength(150);
        builder.Property(l => l.Country).HasMaxLength(150);

        // ImageUrls stored as JSON array
        builder.Property(l => l.ImageUrls)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v.ToList(), (System.Text.Json.JsonSerializerOptions?)null),
                v => (IReadOnlyCollection<string>)System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!
            );

        // Créneaux de disponibilité stockés comme JSON (mêmes contraintes que ImageUrls :
        // pas de filtrage SQL natif possible, filtrage "disponible maintenant" fait en mémoire côté handler)
        builder.Property(l => l.AvailabilitySlots)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v.ToList(), (System.Text.Json.JsonSerializerOptions?)null),
                v => (IReadOnlyCollection<AvailabilitySlot>)System.Text.Json.JsonSerializer.Deserialize<List<AvailabilitySlot>>(v, (System.Text.Json.JsonSerializerOptions?)null)!
            );

        builder.HasOne(l => l.Category)
            .WithMany(c => c.Listings)
            .HasForeignKey(l => l.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.ProviderId);
        builder.HasIndex(l => l.CategoryId);
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.IsActive);
        builder.HasIndex(l => new { l.City, l.Country });

        // Trajets & Capacité
        builder.Property(l => l.DepartureCountryCode).HasMaxLength(10);
        builder.Property(l => l.DepartureCity).HasMaxLength(150);
        builder.Property(l => l.ArrivalCountryCode).HasMaxLength(10);
        builder.Property(l => l.ArrivalCity).HasMaxLength(150);
        builder.Property(l => l.CapacityKg).HasPrecision(18, 2);
        builder.Property(l => l.RemainingKg).HasPrecision(18, 2);
        builder.Property(l => l.PricePerKg).HasPrecision(18, 2);
        builder.Property(l => l.TransportMode).HasMaxLength(50);
        builder.Property(l => l.FlightOrTransportNumber).HasMaxLength(50);
        builder.Property(l => l.ProhibitedItemsDescription).HasMaxLength(2000);
        builder.Property(l => l.MeetingPoints).HasMaxLength(1000);

        builder.HasIndex(l => new { l.DepartureCountryCode, l.ArrivalCountryCode });
        builder.HasIndex(l => l.RemainingKg);
        builder.HasIndex(l => l.DepartureDate);
    }
}