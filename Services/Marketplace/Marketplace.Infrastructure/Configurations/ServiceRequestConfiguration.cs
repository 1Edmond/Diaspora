using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Infrastructure.Configurations;

public class ServiceRequestConfiguration : BaseEntityConfiguration<ServiceRequest>
{
    public override void Configure(EntityTypeBuilder<ServiceRequest> builder)
    {
        base.Configure(builder);

        builder.ToTable("service_requests");

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(r => r.Message).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.AgreedPrice).HasPrecision(18, 2);
        builder.Property(r => r.AgreedCurrency).HasMaxLength(10);
        builder.Property(r => r.WalletTransactionId);
        builder.Property(r => r.CancellationReason).HasMaxLength(500);

        // Colis & Suivi physique
        builder.Property(r => r.WeightKg).HasPrecision(18, 2);
        builder.Property(r => r.PackageDescription).HasMaxLength(1000);
        builder.Property(r => r.PackageContentCategory).HasMaxLength(100);
        builder.Property(r => r.DeclaredValue).HasPrecision(18, 2);
        builder.Property(r => r.HandoverOtpHash).HasMaxLength(500);
        builder.Property(r => r.DeliveryOtpHash).HasMaxLength(500);
        builder.Property(r => r.HandoverOtpCode).HasMaxLength(20);
        builder.Property(r => r.DeliveryOtpCode).HasMaxLength(20);
        builder.Property(r => r.HandoverProofUrl).HasMaxLength(500);
        builder.Property(r => r.DeliveryProofUrl).HasMaxLength(500);
        builder.Property(r => r.HandoverConfirmedAt).HasColumnType("timestamptz");
        builder.Property(r => r.DeliveryConfirmedAt).HasColumnType("timestamptz");

        // Dates
        builder.Property(r => r.AcceptedAt).HasColumnType("timestamptz");
        builder.Property(r => r.CompletedAt).HasColumnType("timestamptz");
        builder.Property(r => r.CancelledAt).HasColumnType("timestamptz");

        // Litiges (Dispute)
        builder.Property(r => r.IsInDispute).IsRequired().HasDefaultValue(false);
        builder.Property(r => r.ActiveDisputeId);

        builder.HasOne(r => r.Listing)
            .WithMany(l => l.Requests)
            .HasForeignKey(r => r.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.RequesterId);
        builder.HasIndex(r => r.ProviderId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.PaymentStatus);
        builder.HasIndex(r => r.ChatThreadId);
        builder.HasIndex(r => r.IsInDispute);
        builder.HasIndex(r => r.ActiveDisputeId);
    }
}
