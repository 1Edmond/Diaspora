using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Infrastructure.Configurations;

public class CategoryConfiguration : BaseEntityConfiguration<Category>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        base.Configure(builder);

        builder.ToTable("categories");

        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(120).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.IconUrl).HasMaxLength(500);
        builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(c => c.DisplayOrder).IsRequired().HasDefaultValue(0);

        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.Name).IsUnique();
        builder.HasIndex(c => c.IsActive);
    }
}
