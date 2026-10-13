using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;

namespace Freelance.Infrastructure.Configurations;

public class JobReviewConfiguration : BaseEntityConfiguration<JobReview>
{
    public override void Configure(EntityTypeBuilder<JobReview> builder)
    {
        base.Configure(builder);
        builder.ToTable("job_reviews");
        builder.Property(r => r.Comment).HasMaxLength(1000);
        builder.HasIndex(r => r.JobApplicationId);
        builder.HasIndex(r => new { r.SubjectId, r.RaterRole });
    }
}