using Freelance.Application.Abstractions;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Freelance.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Handlers.Queries;

public class GetJobCheckInsQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<JobCheckInDto>>> Handle(GetJobCheckInsQuery query)
    {
        IQueryable<Freelance.Domain.Entities.JobCheckIn> q = _context.JobCheckIns
            .Include(c => c.JobApplication);

        if (!query.IsAdmin && query.WorkerId != Guid.Empty)
        {
            q = q.Where(c => c.JobApplication.WorkerId == query.WorkerId);
        }

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => c.MapToDto())
            .ToListAsync();

        return Result.Success(new PagedList<JobCheckInDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}







