using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Infrastructure.Configurations;

public class RefreshTokenConfiguration : BaseEntityConfiguration<RefreshToken>
{
    public override void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.CreatedByIp)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.RevokedByIp)
            .HasMaxLength(50);

        builder.Property(x => x.ReplacedByToken)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasIndex(x => x.ExpiresAt);
    }
}

