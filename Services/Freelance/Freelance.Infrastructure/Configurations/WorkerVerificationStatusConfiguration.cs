using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;

namespace Freelance.Infrastructure.Configurations;

public class WorkerVerificationStatusConfiguration : BaseEntityConfiguration<WorkerVerificationStatus>
{
    public override void Configure(EntityTypeBuilder<WorkerVerificationStatus> builder)
    {
        base.Configure(builder);
        builder.ToTable("worker_verification_statuses");
        builder.Property(s => s.VerificationNotes).HasMaxLength(500);
        builder.HasIndex(s => s.WorkerId).IsUnique();
    }
}