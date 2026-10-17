using Freelance.Application.Abstractions;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Freelance.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Handlers.Queries;

public class GetReceivedJobApplicationsQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<JobApplicationDto>>> Handle(GetReceivedJobApplicationsQuery query)
    {
        IQueryable<Freelance.Domain.Entities.JobApplication> q = _context.JobApplications
            .Include(a => a.Posting);

        if (!query.IsAdmin && query.EmployerId != Guid.Empty)
        {
            q = q.Where(a => a.Posting.EmployerId == query.EmployerId);
        }

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
