using Profile.Application.DTOs;
using Profile.Application.Extensions;
using Profile.Application.Interfaces;
using Profile.Application.Queries;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Handlers.Queries;

public sealed class GetProfileByIdQueryHandler(
    IProfileRepository repository)
{
    public async Task<Result<ProfileDto>> Handle(
        GetProfileByIdQuery query,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetByIdAsync(query.ProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure<ProfileDto>("Profile not found.");

        return Result.Success(profile.ToDto());
    }
}
