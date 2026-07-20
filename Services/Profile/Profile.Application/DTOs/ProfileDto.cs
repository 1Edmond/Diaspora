using Profile.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.DTOs;

public sealed record ProfileDto(
    Guid Id,
    Guid UserId,
    Guid ProfileTypeId,
    string PhoneNumber,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    ProfileStatus Status,
    DateTime? VerifiedAt,
    string? VerifiedByAdminId,
    DateTime CreatedAt
);