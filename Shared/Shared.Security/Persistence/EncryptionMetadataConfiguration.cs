using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Security.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Security.Persistence;

public class EncryptionMetadataConfiguration : IEntityTypeConfiguration<EncryptionMetadataRecord>
{
    public void Configure(EntityTypeBuilder<EncryptionMetadataRecord> builder)
    {

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.PropertyName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.KeyVersion)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Algorithm)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DataType)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.PropertyName })
            .IsUnique();

        builder.HasIndex(x => x.KeyVersion);
        builder.HasIndex(x => new { x.EntityType, x.KeyVersion });
        builder.HasIndex(x => x.EncryptedAt);
    }
}