using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;

namespace Freelance.Infrastructure.Configurations;

public class JobCheckInConfiguration : BaseEntityConfiguration<JobCheckIn>
{
    public override void Configure(EntityTypeBuilder<JobCheckIn> builder)
    {
        base.Configure(builder);
        builder.ToTable("job_check_ins");
        builder.Property(c => c.Notes).HasMaxLength(500);
        builder.HasIndex(c => c.JobApplicationId);
        builder.HasIndex(c => c.Status);
    }
}