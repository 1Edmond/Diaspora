using Document.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;

namespace Document.Infrastructure.Configurations;

public class ProfileDirectoryEntryConfiguration : BaseEntityConfiguration<ProfileDirectoryEntry>
{
    public override void Configure(EntityTypeBuilder<ProfileDirectoryEntry> builder)
    {
        base.Configure(builder);

        builder.ToTable("profile_directory");

        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.DisplayName).HasMaxLength(200);
        builder.Property(e => e.CountryCode).HasMaxLength(2);
        builder.Property(e => e.Currency).HasMaxLength(10);
        builder.Property(e => e.IsActivated).IsRequired();
        builder.Property(e => e.IsVerified).IsRequired();
        builder.Property(e => e.IsSuspended).IsRequired();

        builder.Ignore(e => e.IsEligibleProvider);

        builder.HasIndex(e => e.UserId).IsUnique();
    }
}
