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
    IInternalProfileRepository internalRepo,
    IExternalProfileRepository externalRepo)
{
    public async Task<Result<UserProfilesDto>> Handle(
        GetUserProfilesQuery query,
        CancellationToken cancellationToken)
    {
        var internalProfile = await internalRepo.GetByUserIdAsync(query.UserId, cancellationToken);

        var internalDto = internalProfile?.ToDto();

        var externalProfiles = await externalRepo.GetByUserIdAsync(query.UserId, cancellationToken);
        var externalDtos = externalProfiles.Select(p => p.ToDto()).ToList();

        return Result.Success(new UserProfilesDto(internalDto, externalDtos));
    }
}