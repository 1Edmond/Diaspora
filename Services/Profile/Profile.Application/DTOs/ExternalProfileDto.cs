using Profile.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.DTOs;

public sealed record ExternalProfileDto(
    Guid Id,
    Guid InternalProfileId,
    Guid UserId,
    Guid ExternalProfileTypeId,
    string CountryName,
    string CountryCode,
    string PhoneIndicatif,
    string Currency,
    ExternalProfileKind Kind,
    ExternalProfileStatus Status,
    bool CanOfferServices,
    DateTime? ArrivalValidatedAt,
    DateTime CreatedAt
);
