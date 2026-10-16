using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Enums;

namespace KYC.Infrastructure.Configurations;

public class CountryKycRequirementConfiguration : IEntityTypeConfiguration<CountryKycRequirement>
{
    public void Configure(EntityTypeBuilder<CountryKycRequirement> builder)
    {
        builder.ToTable("country_kyc_requirements");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProfileTypeId).IsRequired();
        builder.Property(x => x.DocumentTypeId).IsRequired();
        builder.Property(x => x.DocumentTypeCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Purpose).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IsRequired).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.TargetLevel)
            .HasConversion<byte>()
            .IsRequired()
            .HasDefaultValue(VerificationLevel.L2_IdentityVerified);
        builder.Property(x => x.RequiredDistinctDocumentsCount)
            .IsRequired()
            .HasDefaultValue(1);

        builder.HasIndex(x => new { x.ProfileTypeId, x.DocumentTypeCode }).IsUnique();
        builder.HasIndex(x => x.IsActive);
    }
}
