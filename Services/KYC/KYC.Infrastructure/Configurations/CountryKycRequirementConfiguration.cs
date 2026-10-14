using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

        builder.HasIndex(x => new { x.ProfileTypeId, x.DocumentTypeCode }).IsUnique();
    }
}
