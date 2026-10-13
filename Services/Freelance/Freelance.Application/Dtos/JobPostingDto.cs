using Freelance.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Dtos;

public record JobPostingDto(
    Guid Id,
    Guid EmployerId,
    string EmployerName,
    Guid? TemplateId,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Description,
    int Capacity,
    int AcceptedCount,
    decimal Amount,
    string Currency,
    PaymentType PaymentType,
    PaymentTiming PaymentTiming,
    CheckInMethod CheckInMethod,
    int? GeofenceRadiusMeters,
    bool RequiresKycVerification,
    DateTime EventStartAt,
    DateTime EventEndAt,
    string? City,
    string? Country,
    double? Latitude,
    double? Longitude,
    bool IsRemote,
    IReadOnlyCollection<string> RequiredSkills,
    IReadOnlyCollection<string> RequiredDocuments,
    DateTime? RegistrationDeadline,
    JobPostingStatus Status,
    DateTime CreatedAt
);