using Profile.Application.DTOs;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Queries;

public sealed record GetUserProfilesQuery(Guid UserId)
    : IQuery<Result<UserProfilesDto>>;
