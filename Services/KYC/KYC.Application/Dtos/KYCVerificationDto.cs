using Shared.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Dtos;

public class KYCVerificationDto : AuditableDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = null!;
    public string Status { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public List<KYCDocumentDto> Documents { get; set; } = new();
}