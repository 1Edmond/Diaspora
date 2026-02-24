using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profile.Domain.Entities;
using Profile.Domain.Enums;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Infrastructure.Configurations;

public sealed class InternalProfileConfiguration : BaseEntityConfiguration<InternalProfile>
{
    public override void Configure(EntityTypeBuilder<InternalProfile> builder)
    {
        base.Configure(builder);

        builder.ToTable("internal_profiles");

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.DateOfBirth)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(InternalProfileStatus.PendingVerification);

        builder.Property(x => x.VerifiedAt)
            .HasColumnType("timestamptz");

        builder.Property(x => x.VerifiedByAdminId)
            .HasMaxLength(255);

        builder.OwnsOne(x => x.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("phone_number")
                .HasMaxLength(20)
                .IsRequired();
        });
        builder.Property(x => x.InternalProfileTypeId).IsRequired();

        builder.HasOne(x => x.InternalProfileType)
            .WithMany()
            .HasForeignKey(x => x.InternalProfileTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ExternalProfiles)
            .WithOne()
            .HasForeignKey(x => x.InternalProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
