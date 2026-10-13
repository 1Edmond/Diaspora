using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;

namespace Freelance.Infrastructure.Configurations;

public class JobCategoryConfiguration : BaseEntityConfiguration<JobCategory>
{
    public override void Configure(EntityTypeBuilder<JobCategory> builder)
    {
        base.Configure(builder);
        builder.ToTable("job_categories");
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.DisplayOrder);
    }
}