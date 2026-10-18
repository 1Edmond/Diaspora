using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;

namespace Marketplace.Infrastructure.Configurations;

public class ProfileTypePaymentPolicyConfiguration : BaseEntityConfiguration<ProfileTypePaymentPolicy>
{
    public override void Configure(EntityTypeBuilder<ProfileTypePaymentPolicy> builder)
    {
        base.Configure(builder);

        builder.ToTable("profile_type_payment_policies");

        builder.Property(p => p.ProfileTypeId).IsRequired();
        builder.Property(p => p.CountryCode).HasMaxLength(10).IsRequired();

        builder.Property(p => p.AllowedPolicy)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(AllowedSettlementPolicy.Both);

        builder.Property(p => p.DefaultCashTiming)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(DirectCashTiming.AtPickup);

        builder.Property(p => p.DirectBookingFeeEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.DirectBookingFeeFixedAmount)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0m);

        builder.Property(p => p.DirectBookingFeeRate)
            .HasPrecision(5, 4)
            .IsRequired()
            .HasDefaultValue(0m);

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(p => p.ProfileTypeId).IsUnique();
        builder.HasIndex(p => p.CountryCode);
    }
}
