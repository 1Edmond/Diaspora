using Freelance.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Commands;

public record UpdateJobPostingCommand(
    Guid Id,
    Guid RequestedBy,
    string Title,
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
    DateTime EventStartAt,
    DateTime EventEndAt,
    string? City = null,
    string? Country = null,
    double? Latitude = null,
    double? Longitude = null,
    bool IsRemote = false,
    IEnumerable<string>? RequiredSkills = null,
    IEnumerable<string>? RequiredDocuments = null,
    DateTime? RegistrationDeadline = null
) : ICommand<Result>;