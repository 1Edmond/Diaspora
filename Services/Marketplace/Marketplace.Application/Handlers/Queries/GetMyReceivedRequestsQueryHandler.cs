using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Extensions;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetMyReceivedRequestsQueryHandler(
    IApplicationDbContext _context,
    IProfileDirectoryRepository _profileDirectory)
{
    public async Task<Result<PagedList<ServiceRequestDto>>> Handle(GetMyReceivedRequestsQuery query)
    {
        var q = _context.ServiceRequests
            .Include(r => r.Listing)
            .Where(r => r.ProviderId == query.ProviderId);

        var totalCount = await q.CountAsync();

        var requests = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var userIds = requests.Select(r => r.RequesterId)
            .Concat(requests.Select(r => r.ProviderId))
            .Distinct()
            .ToList();

        var profiles = await _profileDirectory.GetByUserIdsAsync(userIds);

        var items = requests.Select(r =>
        {
            profiles.TryGetValue(r.RequesterId, out var req);
            profiles.TryGetValue(r.ProviderId, out var prov);
            return r.MapToDto(req, prov, includeOtpCodes: false);
        }).ToList();

        return Result.Success(new PagedList<ServiceRequestDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}
