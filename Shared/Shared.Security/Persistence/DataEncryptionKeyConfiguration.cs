using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Security.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Security.Persistence;

public class DataEncryptionKeyConfiguration : IEntityTypeConfiguration<DataEncryptionKey>
{
    public void Configure(EntityTypeBuilder<DataEncryptionKey> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.EntityType)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.EncryptedKey)
            .IsRequired();

        builder.Property(x => x.KeyVersion)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => new { x.EntityId, x.EntityType, x.IsActive });
    }
}