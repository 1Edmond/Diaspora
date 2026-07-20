using Profile.Application.DTOs;
using Profile.Application.Extensions;
using Profile.Application.Interfaces;
using Profile.Application.Queries;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Handlers.Queries;

public sealed class GetPendingProfilesQueryHandler(
    IProfileRepository repository)
{
    public async Task<Result<PagedList<ProfileDto>>> Handle(
        GetPendingProfilesQuery query,
        CancellationToken cancellationToken)
    {
        var pending = await repository.GetPendingVerificationAsync(cancellationToken);

        var dtos = pending.Select(p => p.ToDto());

        var paged = PagedList<ProfileDto>.Create(dtos, query.PageNumber, query.PageSize);

        return Result.Success(paged);
    }
}