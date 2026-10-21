using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Enums;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Infrastructure.Configurations;

public class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.Property(x => x.KYCLevel)
               .HasConversion<string>()
               .HasMaxLength(50)
               .IsRequired()
               .HasDefaultValue(KYCLevel.None);

        builder.Property(x => x.KYCSubmittedAt);

        builder.Property(x => x.KYCApprovedAt);

        builder.HasIndex(x => x.KYCLevel);

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            email.HasIndex(e => e.Value).IsUnique();
        });

        builder.OwnsOne(x => x.PhoneNumber, phone =>
        {
            phone.Property(e => e.Value)
                .HasColumnName("phone_number")
                .HasMaxLength(20);

            phone.Property(e => e.CountryCode)
                .HasColumnName("phone_country_code")
                .HasMaxLength(5);
        });

        builder.OwnsOne(x => x.FirstName, firstname =>
        {
            firstname.Ignore(e => e.ClearValue);

            firstname.Property(e => e.EncryptedValue)
                .HasColumnName("firstname")
                .HasMaxLength(500);

            firstname.Ignore(e => e.IsEncrypted);
        });

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

      

        builder.Property(x => x.EmailVerificationCode)
            .HasMaxLength(10);


        builder.Property(x => x.LastName)
            .HasMaxLength(100);

        builder.HasMany(x => x.RefreshTokens)
            .WithOne()
            .HasForeignKey("UserId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UserRoles)
         .WithOne(ur => ur.User)
         .HasForeignKey(ur => ur.UserId)
         .OnDelete(DeleteBehavior.Cascade);
        
        builder.Navigation(x => x.RefreshTokens)
            .AutoInclude();

        builder.Navigation(x => x.UserRoles)
            .AutoInclude();
    }
}