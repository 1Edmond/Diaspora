using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Infrastructure.Configurations;

public sealed class ProcedureConfiguration : BaseEntityConfiguration<Domain.Entities.Procedure>
{
    public override void Configure(EntityTypeBuilder<Domain.Entities.Procedure> builder)
    {
        base.Configure(builder);

        builder.ToTable("procedures");

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.ProfileType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProfileTypeId).IsRequired();
        builder.Property(x => x.EstimatedDurationDays).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => new { x.ProfileTypeId, x.ProfileType });

        // Money — owned type
        builder.OwnsOne(x => x.Cost, cost =>
        {
            cost.Property(c => c.Amount)
                .HasColumnName("cost_amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            cost.Property(c => c.Currency)
                .HasColumnName("cost_currency")
                .HasMaxLength(10)
                .IsRequired();
        });

        // N-à-N Locations
        builder.HasMany(x => x.Locations)
            .WithOne()
            .HasForeignKey(x => x.ProcedureId)
            .OnDelete(DeleteBehavior.Cascade);

        // Dependencies
        builder.HasMany(x => x.Dependencies)
            .WithOne()
            .HasForeignKey(x => x.ProcedureId)
            .OnDelete(DeleteBehavior.Cascade);

        // Required Documents
        builder.HasMany(x => x.RequiredDocuments)
            .WithOne()
            .HasForeignKey(x => x.ProcedureId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
