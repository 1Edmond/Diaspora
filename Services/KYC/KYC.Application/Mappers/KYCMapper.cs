using KYC.Application.Dtos;
using KYC.Domain.Entities;
using Shared.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Mappers;

public static class KYCMapper
{
    public static KYCVerificationDto MapToDto(KYCVerification verification)
    {
        return new KYCVerificationDto
        {
            Id = verification.Id,
            UserId = verification.UserId,
            FirstName = verification.FirstName.ClearValue,
            LastName = verification.LastName.ClearValue,
            DateOfBirth = verification.DateOfBirth.ClearValue,
            Nationality = verification.Nationality.ClearValue,
            Address = new AddressDto
            {
                Street = verification.Address.Street,
                City = verification.Address.City,
                State = verification.Address.State,
                PostalCode = verification.Address.PostalCode,
                Country = verification.Address.Country
            },
            Status = verification.Status.ToString(),
            Level = verification.Level.ToString(),
            RejectionReason = verification.RejectionReason,
            SubmittedAt = verification.SubmittedAt,
            ReviewedAt = verification.ReviewedAt,
            Documents = [.. verification.Documents.Select(d => new KYCDocumentDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                FilePath = d.FilePath,
                FileName = d.FileName,
                FileSize = d.FileSize,
                UploadedAt = d.UploadedAt
            })],
            CreatedAt = verification.CreatedAt,
            UpdatedAt = verification.UpdatedAt,
            CreatedBy = verification.CreatedBy,
            UpdatedBy = verification.UpdatedBy
        };
    }
}
