using Freelance.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Dtos;

public record JobTemplateDto(
    Guid Id,
    Guid EmployerId,
    string Name,
    string Description,
    Guid CategoryId,
    string CategoryName,
    int Capacity,
    decimal Amount,
    string Currency,
    PaymentType PaymentType,
    PaymentTiming PaymentTiming,
    CheckInMethod CheckInMethod,
    int? GeofenceRadiusMeters,
    bool RequiresKycVerification,
    IReadOnlyCollection<string> RequiredSkills,
    IReadOnlyCollection<string> RequiredDocuments,
    DateTime CreatedAt
);