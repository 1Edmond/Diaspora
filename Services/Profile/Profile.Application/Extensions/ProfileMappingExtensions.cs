using Profile.Application.DTOs;
using Profile.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Extensions;

public static class ProfileMappingExtensions
{
    public static ProfileDto ToDto(this Domain.Entities.Profile profile) =>
        new(
            profile.Id,
            profile.UserId,
            profile.ProfileTypeId,
            profile.PhoneNumber.Value,
            profile.FirstName,
            profile.LastName,
            profile.DateOfBirth,
            profile.Status,
            profile.VerifiedAt,
            profile.VerifiedByAdminId,
            profile.CreatedAt);
}
