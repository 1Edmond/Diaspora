using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profile.Domain.Entities;
using Profile.Domain.Enums;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Infrastructure.Configurations;

public sealed class InternalProfileConfiguration : BaseEntityConfiguration<Domain.Entities.Profile>
{
    public override void Configure(EntityTypeBuilder<Domain.Entities.Profile> builder)
    {
        base.Configure(builder);

        builder.ToTable("profiles");

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

        builder.Property(x => x.Kind)
           .HasConversion<string>()
           .HasMaxLength(50)
           .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(ProfileStatus.PendingVerification);

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
        builder.Property(x => x.ProfileTypeId).IsRequired();

        builder.Property(x => x.Level)
            .HasConversion<byte>()
            .IsRequired()
            .HasDefaultValue(Shared.Domain.Enums.VerificationLevel.L0_Registered);

        builder.HasIndex(x => x.Level);

        builder.HasOne(x => x.ProfileType)
            .WithMany()
            .HasForeignKey(x => x.ProfileTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
