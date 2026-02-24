using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Procedure.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Infrastructure.Configurations;

public sealed class UserProcedureConfiguration : BaseEntityConfiguration<UserProcedure>
{
    public override void Configure(EntityTypeBuilder<UserProcedure> builder)
    {
        base.Configure(builder);

        builder.ToTable("user_procedures");

        builder.HasIndex(x => new { x.ProfileId, x.ProcedureId }).IsUnique();
        builder.HasIndex(x => x.ProfileId);
        builder.HasIndex(x => x.Status);

        builder.Property(x => x.ProfileId).IsRequired();
        builder.Property(x => x.ProcedureId).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(UserProcedureStatus.NotStarted);

        builder.Property(x => x.CompletedByUserId).HasMaxLength(255);
        builder.Property(x => x.CompletedByRole).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.CompletedAt).HasColumnType("timestamptz");
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne(x => x.Procedure)
            .WithMany()
            .HasForeignKey(x => x.ProcedureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

