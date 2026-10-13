using Freelance.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Commands;

public record UpdateJobTemplateCommand(
    Guid Id,
    Guid RequestedBy,
    string Name,
    string Description,
    Guid CategoryId,
    int Capacity,
    decimal Amount,
    string Currency,
    PaymentType PaymentType,
    PaymentTiming PaymentTiming,
    CheckInMethod CheckInMethod,
    int? GeofenceRadiusMeters,
    bool RequiresKycVerification,
    IEnumerable<string>? RequiredSkills = null,
    IEnumerable<string>? RequiredDocuments = null
) : ICommand<Result>;