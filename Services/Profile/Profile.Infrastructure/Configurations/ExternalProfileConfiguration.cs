using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profile.Domain.Entities;
using Profile.Domain.Enums;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Infrastructure.Configurations;

public sealed class ExternalProfileConfiguration : BaseEntityConfiguration<ExternalProfile>
{
    public override void Configure(EntityTypeBuilder<ExternalProfile> builder)
    {
        base.Configure(builder);

        builder.ToTable("external_profiles");

        // Un seul profil externe par user ET par type de pays
        builder.HasIndex(x => new { x.UserId, x.ExternalProfileTypeId }).IsUnique();
        builder.HasIndex(x => x.InternalProfileId);

        builder.Property(x => x.InternalProfileId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ExternalProfileTypeId).IsRequired();

        builder.Property(x => x.Kind)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(ExternalProfileStatus.PendingArrival);

        builder.Property(x => x.ArrivalKycVerificationId);

        builder.Property(x => x.ArrivalValidatedAt)
            .HasColumnType("timestamptz");

        builder.HasOne(x => x.ExternalProfileType)
            .WithMany()
            .HasForeignKey(x => x.ExternalProfileTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

