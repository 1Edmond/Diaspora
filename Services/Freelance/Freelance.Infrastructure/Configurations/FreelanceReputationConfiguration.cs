using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;

namespace Freelance.Infrastructure.Configurations;

public class FreelanceReputationConfiguration : BaseEntityConfiguration<FreelanceReputation>
{
    public override void Configure(EntityTypeBuilder<FreelanceReputation> builder)
    {
        base.Configure(builder);
        builder.ToTable("freelance_reputations");
        builder.Property(r => r.AverageRating).HasPrecision(3, 2);
        builder.Property(r => r.AveragePunctuality).HasPrecision(3, 2);
        builder.Property(r => r.AverageQuality).HasPrecision(3, 2);
        builder.Property(r => r.AverageCommunication).HasPrecision(3, 2);
        builder.HasIndex(r => new { r.SubjectId, r.Role }).IsUnique();
    }
}