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

        var categories = await q
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        if (query.ProfileTypeId.HasValue)
        {
            categories = categories
                .Where(c => c.AllowedProfileTypeIds == null 
                         || !c.AllowedProfileTypeIds.Any() 
                         || c.AllowedProfileTypeIds.Contains(query.ProfileTypeId.Value))
                .ToList();
        }

        var items = categories.Select(c => c.MapToDto()).ToList();

        return Result.Success(items);
    }
}







