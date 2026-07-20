using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.DTOs;

public sealed record UserProfilesDto(
    IReadOnlyList<ProfileDto>? Profiles
);