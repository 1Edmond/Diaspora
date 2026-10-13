using Freelance.Application.Abstractions;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Freelance.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Handlers.Queries;

public class GetMyJobApplicationsQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<JobApplicationDto>>> Handle(GetMyJobApplicationsQuery query)
    {
        var q = _context.JobApplications
            .Include(a => a.Posting)
            .Where(a => a.WorkerId == query.WorkerId);

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderByDescending(a => a.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => a.MapToDto())
            .ToListAsync();

        return Result.Success(new PagedList<JobApplicationDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}







