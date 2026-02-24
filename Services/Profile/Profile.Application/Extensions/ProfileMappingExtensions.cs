using Profile.Application.DTOs;
using Profile.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Extensions;

public static class ProfileMappingExtensions
{
    public static InternalProfileDto ToDto(this InternalProfile profile) =>
        new(
            profile.Id,
            profile.UserId,
            profile.InternalProfileTypeId,
            profile.PhoneNumber.Value,
            profile.FirstName,
            profile.LastName,
            profile.DateOfBirth,
            profile.Status,
            profile.VerifiedAt,
            profile.VerifiedByAdminId,
            profile.CreatedAt);

    public static ExternalProfileDto ToDto(this ExternalProfile profile) =>
        new(
            profile.Id,
            profile.InternalProfileId,
            profile.UserId,
            profile.ExternalProfileTypeId,
            profile.ExternalProfileType?.CountryName ?? string.Empty,
            profile.ExternalProfileType?.CountryCode ?? string.Empty,
            profile.ExternalProfileType?.PhoneIndicatif ?? string.Empty,
            profile.ExternalProfileType?.Currency ?? string.Empty,
            profile.Kind,
            profile.Status,
            profile.CanOfferServices,
            profile.ArrivalValidatedAt,
            profile.CreatedAt);
}
