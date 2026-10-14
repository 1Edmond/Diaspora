using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;

namespace Marketplace.Infrastructure.Configurations;

public class CategoryPolicyConfiguration : BaseEntityConfiguration<CategoryPolicy>
{
    public override void Configure(EntityTypeBuilder<CategoryPolicy> builder)
    {
        base.Configure(builder);

        builder.ToTable("category_policies");

        builder.Property(p => p.CategoryId).IsRequired();
        builder.Property(p => p.MinPublishLevel).IsRequired();
        builder.Property(p => p.MinBookLevel).IsRequired();
        builder.Property(p => p.MaxWeightKg).HasPrecision(10, 2);
        builder.Property(p => p.MaxValueAmount).HasPrecision(18, 2);
        builder.Property(p => p.RequiresEscrow).IsRequired();
        builder.Property(p => p.RequiresPreModeration).IsRequired();
        builder.Property(p => p.DefaultCommissionRate).HasPrecision(5, 4).IsRequired();

        builder.HasIndex(p => p.CategoryId).IsUnique();

        builder.HasOne(p => p.Category)
            .WithOne(c => c.Policy)
            .HasForeignKey<CategoryPolicy>(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
