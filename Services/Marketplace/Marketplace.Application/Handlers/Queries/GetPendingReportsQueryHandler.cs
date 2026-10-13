using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
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

public class GetPendingReportsQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<ReportDto>>> Handle(GetPendingReportsQuery query)
    {
        var q = _context.Reports
            .Where(r => r.Status == ReportStatus.Pending)
            .OrderBy(r => r.CreatedAt);

        var totalCount = await q.CountAsync();

        var items = await q
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new ReportDto(
                r.Id, r.TargetType, r.TargetId, r.ReporterId, r.Reason, r.Status,
                r.ReviewedBy, r.ReviewedAt, r.ResolutionNotes, r.CreatedAt
            ))
            .ToListAsync();

        return Result.Success(new PagedList<ReportDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}