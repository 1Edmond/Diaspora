using Freelance.Application.Abstractions;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Freelance.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Handlers.Queries;

public class GetMyJobPostingsQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<JobPostingSummaryDto>>> Handle(GetMyJobPostingsQuery query)
    {
        var q = _context.JobPostings
            .Include(p => p.Category)
            .Where(p => p.EmployerId == query.EmployerId);

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => p.MapToSummaryDto())
            .ToListAsync();

        return Result.Success(new PagedList<JobPostingSummaryDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}







