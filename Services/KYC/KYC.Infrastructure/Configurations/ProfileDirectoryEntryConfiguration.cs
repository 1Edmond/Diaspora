using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KYC.Infrastructure.Configurations;

public class ProfileDirectoryEntryConfiguration : IEntityTypeConfiguration<ProfileDirectoryEntry>
{
    public void Configure(EntityTypeBuilder<ProfileDirectoryEntry> builder)
    {
        builder.ToTable("profile_directory_entries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.Property(x => x.DisplayName).HasMaxLength(255);
        builder.Property(x => x.CountryCode).HasMaxLength(2);
        builder.Property(x => x.Currency).HasMaxLength(3);
    }
}
