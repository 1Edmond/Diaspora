using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Infrastructure.Configurations;

public class KYCVerificationConfiguration : BaseEntityConfiguration<KYCVerification>
{
    public override void Configure(EntityTypeBuilder<KYCVerification> builder)
    {
        builder.ToTable("kyc_verifications");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.FirstName, firstName =>
        {
            firstName.Ignore(e => e.ClearValue);
            firstName.Property(e => e.EncryptedValue)
                .HasColumnName("first_name")
                .HasMaxLength(500)
                .IsRequired();
            firstName.Ignore(e => e.IsEncrypted);
        });

        builder.OwnsOne(x => x.LastName, lastName =>
        {
            lastName.Ignore(e => e.ClearValue);
            lastName.Property(e => e.EncryptedValue)
                .HasColumnName("last_name")
                .HasMaxLength(500)
                .IsRequired();
            lastName.Ignore(e => e.IsEncrypted);
        });

        builder.OwnsOne(x => x.DateOfBirth, dob =>
        {
            dob.Ignore(e => e.ClearValue);
            dob.Property(e => e.EncryptedValue)
                .HasColumnName("date_of_birth")
                .HasMaxLength(500)
                .IsRequired();
            dob.Ignore(e => e.IsEncrypted);
        });

        builder.OwnsOne(x => x.Nationality, nationality =>
        {
            nationality.Ignore(e => e.ClearValue);
            nationality.Property(e => e.EncryptedValue)
                .HasColumnName("nationality")
                .HasMaxLength(500)
                .IsRequired();
            nationality.Ignore(e => e.IsEncrypted);
        });

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("address_street")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(a => a.City)
                .HasColumnName("address_city")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.State)
                .HasColumnName("address_state")
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("address_postal_code")
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .HasColumnName("address_country")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Level)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(x => x.SubmittedAt);
        builder.Property(x => x.ReviewedAt);
        builder.Property(x => x.ReviewedBy);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.DeletedAt);

        builder.Property(x => x.DeletedBy)
            .HasMaxLength(100);

        builder.HasMany(x => x.Documents)
            .WithOne()
            .HasForeignKey(d => d.VerificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Level);
        builder.HasIndex(x => x.SubmittedAt);
        builder.HasIndex(x => x.IsDeleted);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}