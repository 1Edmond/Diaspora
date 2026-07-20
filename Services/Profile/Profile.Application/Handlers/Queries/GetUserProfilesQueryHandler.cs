using Profile.Application.DTOs;
using Profile.Application.Extensions;
using Profile.Application.Interfaces;
using Profile.Application.Queries;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Handlers.Queries;

public sealed class GetUserProfilesQueryHandler(
    IProfileRepository internalRepo)
{
    public async Task<Result<UserProfilesDto>> Handle(
        GetUserProfilesQuery query,
        CancellationToken cancellationToken)
    {
        var profiles = await internalRepo.GetByUserIdAsync(query.UserId, cancellationToken);

        var profilesDto = profiles?.Select(p => p.ToDto()).ToList();

        return Result.Success(new UserProfilesDto(profilesDto));
    }
}