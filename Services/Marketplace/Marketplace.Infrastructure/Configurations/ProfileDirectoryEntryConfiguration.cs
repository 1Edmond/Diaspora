using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Enums;
using Shared.Infrastructure.Persistence.Configurations;

namespace Marketplace.Infrastructure.Configurations;

public class ProfileDirectoryEntryConfiguration : BaseEntityConfiguration<ProfileDirectoryEntry>
{
    public override void Configure(EntityTypeBuilder<ProfileDirectoryEntry> builder)
    {
        base.Configure(builder);

        builder.ToTable("profile_directory");

        // L'Id est le ProfileId fourni par le Profile Service : jamais généré localement.
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.DisplayName).HasMaxLength(200);
        builder.Property(e => e.CountryCode).HasMaxLength(2);   // aligné sur ProfileType.CountryCode
        builder.Property(e => e.Currency).HasMaxLength(10);     // aligné sur ProfileType.Currency
        builder.Property(e => e.Level).IsRequired().HasDefaultValue(VerificationLevel.L0_Registered);
        builder.Property(e => e.IsActivated).IsRequired();
        builder.Property(e => e.IsVerified).IsRequired();
        builder.Property(e => e.IsSuspended).IsRequired();
        builder.Property(e => e.IsKycApproved).IsRequired().HasDefaultValue(false);

        // Propriété calculée : non persistée.
        builder.Ignore(e => e.IsEligibleProvider);

        // Un utilisateur = un profil dans cette projection.
        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasIndex(e => e.Level);
        builder.HasIndex(e => e.IsKycApproved);
    }
}
