using Freelance.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record CreateJobTemplateCommand(
    Guid EmployerId,
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
) : ICommand<Result<Guid>>;