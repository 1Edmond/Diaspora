using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;

namespace Marketplace.Infrastructure.Configurations;

public class DisputeConfiguration : BaseEntityConfiguration<Dispute>
{
    public override void Configure(EntityTypeBuilder<Dispute> builder)
    {
        base.Configure(builder);

        builder.ToTable("disputes");

        builder.Property(d => d.ServiceRequestId).IsRequired();
        builder.Property(d => d.InitiatorId).IsRequired();
        builder.Property(d => d.DefendantId).IsRequired();
        builder.Property(d => d.Reason).HasMaxLength(500).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(2000).IsRequired();
        builder.Property(d => d.ResolutionNotes).HasMaxLength(2000);
        builder.Property(d => d.BuyerRefundPercentage).HasPrecision(5, 2);
        builder.Property(d => d.SellerPayoutPercentage).HasPrecision(5, 2);

        builder.Property(d => d.EvidenceUrls)
            .HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries))
            .HasMaxLength(2000);

        builder.HasIndex(d => d.ServiceRequestId);
        builder.HasIndex(d => d.InitiatorId);
        builder.HasIndex(d => d.Status);

        builder.HasOne(d => d.ServiceRequest)
            .WithMany()
            .HasForeignKey(d => d.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
