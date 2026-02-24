using Profile.Application.DTOs;
using Profile.Application.Extensions;
using Profile.Application.Interfaces;
using Profile.Application.Queries;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Handlers.Queries;

public sealed class GetInternalProfileByUserIdQueryHandler(
    IInternalProfileRepository repository)
{
    public async Task<Result<InternalProfileDto>> Handle(
        GetInternalProfileByUserIdQuery query,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetByUserIdAsync(query.UserId, cancellationToken);
        if (profile is null)
            return Result.Failure<InternalProfileDto>("Internal profile not found.");

        return Result.Success(profile.ToDto());
    }
}