using Profile.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.DTOs;

public sealed record InternalProfileDto(
    Guid Id,
    Guid UserId,
    Guid InternalProfileTypeId,
    string PhoneNumber,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    InternalProfileStatus Status,
    DateTime? VerifiedAt,
    string? VerifiedByAdminId,
    DateTime CreatedAt
);