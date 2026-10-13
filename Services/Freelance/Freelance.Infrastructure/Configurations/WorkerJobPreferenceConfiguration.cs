using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;

namespace Freelance.Infrastructure.Configurations;

public class WorkerJobPreferenceConfiguration : BaseEntityConfiguration<WorkerJobPreference>
{
    public override void Configure(EntityTypeBuilder<WorkerJobPreference> builder)
    {
        base.Configure(builder);
        builder.ToTable("worker_job_preferences");
        builder.Property(p => p.PreferredCity).HasMaxLength(150);
        builder.Property(p => p.PreferredCountry).HasMaxLength(150);

        builder.Property(p => p.PreferredCategoryIds)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v.ToList(), (System.Text.Json.JsonSerializerOptions?)null),
                v => (IReadOnlyCollection<Guid>)System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null)!);

        builder.HasIndex(p => p.WorkerId).IsUnique();
    }
}