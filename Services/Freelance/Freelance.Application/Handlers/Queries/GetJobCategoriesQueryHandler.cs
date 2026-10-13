using Freelance.Application.Abstractions;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Freelance.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Handlers.Queries;

public class GetJobCategoriesQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<List<JobCategoryDto>>> Handle(GetJobCategoriesQuery query)
    {
        var q = _context.JobCategories.AsQueryable();

        if (query.ActiveOnly)
            q = q.Where(c => c.IsActive);

        var items = await q
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => c.MapToDto())
            .ToListAsync();

        return Result.Success(items);
    }
}







