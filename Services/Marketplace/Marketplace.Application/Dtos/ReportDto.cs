using Marketplace.Domain.Enums;
using System;

namespace Marketplace.Application.Dtos;

public record ReportDto(
    Guid Id,
    ReportTargetType TargetType,
    Guid TargetId,
    Guid ReporterId,
    string Reason,
    ReportStatus Status,
    Guid? ReviewedBy,
    DateTime? ReviewedAt,
    string? ResolutionNotes,
    DateTime CreatedAt
);