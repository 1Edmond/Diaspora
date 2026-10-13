using Freelance.Domain.Enums;
using System;

namespace Freelance.Application.Dtos;

public record JobCheckInDto(
    Guid Id,
    Guid JobApplicationId,
    string JobPostingTitle,
    Guid WorkerId,
    string WorkerName,
    CheckInMethod Method,
    CheckInStatus Status,
    DateTime? CheckedInAt,
    DateTime? CheckedOutAt,
    double? Latitude,
    double? Longitude,
    string? Notes,
    DateTime CreatedAt
);