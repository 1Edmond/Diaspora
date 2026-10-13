using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Infrastructure.Configurations;

public class FavoriteConfiguration : BaseEntityConfiguration<Favorite>
{
    public override void Configure(EntityTypeBuilder<Favorite> builder)
    {
        base.Configure(builder);

        builder.ToTable("favorites");

        builder.HasOne(f => f.Listing)
            .WithMany()
            .HasForeignKey(f => f.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.UserId, f.ListingId }).IsUnique();
        builder.HasIndex(f => f.UserId);
    }
}