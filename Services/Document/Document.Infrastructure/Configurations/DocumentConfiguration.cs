using Document.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Infrastructure.Configurations;

public sealed class DocumentConfiguration
    : BaseEntityConfiguration<Domain.Entities.Document>
{
    public override void Configure(EntityTypeBuilder<Domain.Entities.Document> builder)
    {
        base.Configure(builder);

        builder.ToTable ("documents");

        builder.HasIndex(x => x.ProfileId);
        builder.HasIndex(x => x.DocumentTypeId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ExpiresAt);

        builder.Property(x => x.ProfileId).IsRequired();
        builder.Property(x => x.DocumentTypeId).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(DocumentStatus.Pending);

        builder.Property(x => x.FilePath).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.FileSize).IsRequired();
        builder.Property(x => x.MimeType).HasMaxLength(100).IsRequired();

        builder.Property(x => x.ExpiresAt).HasColumnType("timestamptz");
        builder.Property(x => x.IssuedAt).HasColumnType("timestamptz");
        builder.Property(x => x.IssuedBy).HasMaxLength(255);
        builder.Property(x => x.RejectionReason).HasMaxLength(500);
        builder.Property(x => x.ValidatedByAdminId).HasMaxLength(255);
        builder.Property(x => x.ValidatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.RenewalAlertSentAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.DocumentType)
            .WithMany()
            .HasForeignKey(x => x.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
