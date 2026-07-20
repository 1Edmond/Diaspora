using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Procedure.Domain.Entities;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Infrastructure.Configurations;

public sealed class ProcedureLocationConfiguration : BaseEntityConfiguration<ProcedureLocation>
{
    public override void Configure(EntityTypeBuilder<ProcedureLocation> builder)
    {
        base.Configure(builder);

        builder.ToTable("procedure_locations");

        builder.HasIndex(x => new { x.ProcedureId, x.LocationId }).IsUnique();

        builder.Property(x => x.ProcedureId).IsRequired();
        builder.Property(x => x.LocationId).IsRequired();

        builder.HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}