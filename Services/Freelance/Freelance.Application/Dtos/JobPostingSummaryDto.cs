using Freelance.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Dtos;

public record JobPostingSummaryDto(
    Guid Id,
    Guid EmployerId,
    string EmployerName,
    Guid CategoryId,
    string CategoryName,
    string Title,
    decimal Amount,
    string Currency,
    PaymentType PaymentType,
    PaymentTiming PaymentTiming,
    DateTime EventStartAt,
    DateTime EventEndAt,
    string? City,
    string? Country,
    double? DistanceKm,
    int Capacity,
    int AcceptedCount,
    JobPostingStatus Status,
    DateTime CreatedAt
);