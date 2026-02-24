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

        builder.HasIndex(x => x.ProfileTypeId);
        builder.HasIndex(x => x.ProfileType);

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ProfileType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProfileTypeId).IsRequired();
        builder.Property(x => x.EstimatedDurationDays).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        // Money — owned type (2 colonnes : cost_amount, cost_currency)
        builder.OwnsOne(x => x.Cost, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("cost_amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("cost_currency")
                .HasMaxLength(10)
                .IsRequired();
        });

        // Location FK optionnelle
        builder.Property(x => x.LocationId);
        builder.HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Dependencies
        builder.HasMany(x => x.Dependencies)
            .WithOne(d => d.DependsOnProcedure)
            .HasForeignKey(d => d.ProcedureId)
            .OnDelete(DeleteBehavior.Cascade);

      
        builder.HasMany(x => x.RequiredDocuments)
            .WithOne()
            .HasForeignKey(d => d.ProcedureId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
