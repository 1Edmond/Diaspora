using Profile.Application.DTOs;
using Profile.Application.Extensions;
using Profile.Application.Interfaces;
using Profile.Application.Queries;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Profile.Application.Handlers.Queries;

public sealed class GetProfilesQueryHandler(
    IProfileRepository repository)
{
    public async Task<Result<PagedList<ProfileDto>>> Handle(
        GetProfilesQuery query,
        CancellationToken cancellationToken)
    {
        var allProfiles = await repository.GetAllAsync(cancellationToken);
        
        var dtos = allProfiles.Select(p => p.ToDto()).ToList();
        
        var queryable = dtos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLower();
            queryable = queryable.Where(p => 
                (p.FirstName != null && p.FirstName.ToLower().Contains(term)) ||
                (p.LastName != null && p.LastName.ToLower().Contains(term)) ||
                (p.PhoneNumber != null && p.PhoneNumber.ToLower().Contains(term)) ||
                p.Id.ToString().ToLower().Contains(term) ||
                p.UserId.ToString().ToLower().Contains(term));
        }

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(p => p.Status == query.Status.Value);
        }

        if (query.ProfileTypeId.HasValue)
        {
            queryable = queryable.Where(p => p.ProfileTypeId == query.ProfileTypeId.Value);
        }

        var list = queryable.OrderByDescending(p => p.CreatedAt).ToList();
        
        var paged = PagedList<ProfileDto>.Create(list, query.PageNumber, query.PageSize);

        return Result.Success(paged);
    }
}
