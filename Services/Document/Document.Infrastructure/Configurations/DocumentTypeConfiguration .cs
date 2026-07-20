using Document.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Messaging.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Infrastructure.Configurations;

public sealed class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable("document_types");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsRenewable).IsRequired();
        builder.Property(x => x.RenewalAlertDays).IsRequired().HasDefaultValue(30);
        builder.Property(x => x.IsRequired).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(255).IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.UpdatedBy).HasMaxLength(255);


        builder.HasData(
            new
            {
                Id = ServiceDataConstants.VisaDocumentTypeId,
                ProfileTypeId = ServiceDataConstants.TgInternalProfileTypeId,
                Name = "Visa",
                Code = "VISA",
                Description = "Visa d'entrée et de séjour",
                IsRenewable = true,
                RenewalAlertDays = 60,
                IsRequired = true,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "System",
                UpdatedAt = (DateTime?)null,
                UpdatedBy = (string?)null
            },
            new
            {
                Id = ServiceDataConstants.StudentCardDocumentTypeId,
                ProfileTypeId = ServiceDataConstants.TgInternalProfileTypeId,
                Name = "Carte étudiant",
                Code = "STUDENT_CARD",
                Description = "Carte d'identité étudiante universitaire",
                IsRenewable = true,
                RenewalAlertDays = 30,
                IsRequired = false,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "System",
                UpdatedAt = (DateTime?)null,
                UpdatedBy = (string?)null
            },
            new
            {
                Id = ServiceDataConstants.MedicalInsurranceDocumentTypeId,
                ProfileTypeId = ServiceDataConstants.TgInternalProfileTypeId,
                Name = "Assurance médicale",
                Code = "MEDICAL_INSURANCE",
                Description = "Assurance maladie obligatoire",
                IsRenewable = true,
                RenewalAlertDays = 45,
                IsRequired = true,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "System",
                UpdatedAt = (DateTime?)null,
                UpdatedBy = (string?)null
            },
            new
            {
                Id = ServiceDataConstants.MedicalCertificateDocumentTypeId,
                ProfileTypeId = ServiceDataConstants.TgInternalProfileTypeId,
                Name = "Certificat médical",
                Code = "MEDICAL_CERTIFICATE",
                Description = "Certificat médical d'aptitude",
                IsRenewable = false,
                RenewalAlertDays = 0,
                IsRequired = false,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "System",
                UpdatedAt = (DateTime?)null,
                UpdatedBy = (string?)null
            }
        );
    }
}
