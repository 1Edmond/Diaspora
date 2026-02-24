using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Infrastructure.Configurations;

public class KYCDocumentConfiguration : BaseEntityConfiguration<KYCDocument>
{
    public override void Configure(EntityTypeBuilder<KYCDocument> builder)
    {
        builder.ToTable("kyc_documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.VerificationId)
            .IsRequired();

        builder.Property(x => x.DocumentType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.FilePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.UploadedAt)
            .IsRequired();

        builder.HasIndex(x => x.VerificationId);
        builder.HasIndex(x => x.DocumentType);
    }
}
