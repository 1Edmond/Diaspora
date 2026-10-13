using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;

namespace Freelance.Infrastructure.Configurations;

public class JobApplicationConfiguration : BaseEntityConfiguration<JobApplication>
{
    public override void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        base.Configure(builder);
        builder.ToTable("job_applications");
        builder.Property(a => a.Message).HasMaxLength(1000);
        builder.Property(a => a.RejectionReason).HasMaxLength(500);

        builder.HasOne(a => a.Posting)
            .WithMany()
            .HasForeignKey(a => a.JobPostingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.WorkerId);
        builder.HasIndex(a => a.JobPostingId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.ChatThreadId);
        builder.HasIndex(a => a.EscrowTransactionId);
    }
}