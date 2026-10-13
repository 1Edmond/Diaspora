using Freelance.Domain.Enums;
using System;

namespace Freelance.Application.Dtos;

public record JobApplicationDto(
    Guid Id,
    Guid JobPostingId,
    string JobPostingTitle,
    Guid WorkerId,
    string WorkerName,
    Guid EmployerId,
    string EmployerName,
    string? Message,
    JobApplicationStatus Status,
    DateTime? DecidedAt,
    Guid? DecidedBy,
    string? RejectionReason,
    Guid? ChatThreadId,
    Guid? EscrowTransactionId,
    DateTime CreatedAt
);