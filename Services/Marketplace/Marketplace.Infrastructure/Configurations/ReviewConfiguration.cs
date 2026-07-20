using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Infrastructure.Configurations;

public class ReviewConfiguration : BaseEntityConfiguration<Review>
{
    public override void Configure(EntityTypeBuilder<Review> builder)
    {
        base.Configure(builder);

        builder.ToTable("reviews");

        builder.Property(r => r.Comment).HasMaxLength(1000).IsRequired();

        builder.HasOne(r => r.Listing)
            .WithMany(l => l.Reviews)
            .HasForeignKey(r => r.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Un reviewer ne peut avoir qu'un avis par listing
        builder.HasIndex(r => new { r.ListingId, r.ReviewerId }).IsUnique();
        builder.HasIndex(r => r.ReviewerId);
    }
}