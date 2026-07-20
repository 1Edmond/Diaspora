using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Extensions;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetMyReceivedRequestsQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<ServiceRequestDto>>> Handle(GetMyReceivedRequestsQuery query)
    {
        var q = _context.ServiceRequests
            .Include(r => r.Listing)
            .Where(r => r.ProviderId == query.ProviderId);

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => r.MapToDto())
            .ToListAsync();

        return Result.Success(new PagedList<ServiceRequestDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}
