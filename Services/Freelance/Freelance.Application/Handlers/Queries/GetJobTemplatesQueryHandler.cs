using Freelance.Application.Abstractions;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Freelance.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Handlers.Queries;

public class GetJobTemplatesQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<JobTemplateDto>>> Handle(GetJobTemplatesQuery query)
    {
        var q = _context.JobTemplates
            .Include(t => t.Category)
            .Where(t => t.EmployerId == query.EmployerId);

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderByDescending(t => t.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => t.MapToDto())
            .ToListAsync();

        return Result.Success(new PagedList<JobTemplateDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}







