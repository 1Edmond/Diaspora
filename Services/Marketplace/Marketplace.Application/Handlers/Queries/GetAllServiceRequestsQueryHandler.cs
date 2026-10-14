using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Extensions;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetAllServiceRequestsQueryHandler(
    IApplicationDbContext _context,
    IProfileDirectoryRepository _profileDirectory)
{
    public async Task<Result<PagedList<ServiceRequestDto>>> Handle(GetAllServiceRequestsQuery query)
    {
        var q = _context.ServiceRequests
            .Include(r => r.Listing)
            .AsQueryable();

        if (query.Status.HasValue)
            q = q.Where(r => r.Status == query.Status.Value);

        if (query.PaymentStatus.HasValue)
            q = q.Where(r => r.PaymentStatus == query.PaymentStatus.Value);

        if (query.ProviderId.HasValue)
            q = q.Where(r => r.ProviderId == query.ProviderId.Value);

        if (query.RequesterId.HasValue)
            q = q.Where(r => r.RequesterId == query.RequesterId.Value);

        if (query.DateFrom.HasValue)
            q = q.Where(r => r.CreatedAt >= query.DateFrom.Value);

        if (query.DateTo.HasValue)
            q = q.Where(r => r.CreatedAt <= query.DateTo.Value);

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
            return r.MapToDto(req, prov);
        }).ToList();

        return Result.Success(new PagedList<ServiceRequestDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}