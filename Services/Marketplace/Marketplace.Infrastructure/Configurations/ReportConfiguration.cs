using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Infrastructure.Configurations;

public class ReportConfiguration : BaseEntityConfiguration<Report>
{
    public override void Configure(EntityTypeBuilder<Report> builder)
    {
        base.Configure(builder);

        builder.ToTable("reports");

        builder.Property(r => r.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.ResolutionNotes).HasMaxLength(1000);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => new { r.TargetType, r.TargetId });
        builder.HasIndex(r => r.ReporterId);
    }
}