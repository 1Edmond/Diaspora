using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetAllCategoriesQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<List<CategoryDto>>> Handle(GetAllCategoriesQuery query)
    {
        var categories = await _context.Categories
            .Where(c => !query.ActiveOnly || c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.IconUrl,
                c.IsActive,
                c.DisplayOrder,
                c.Listings.Count(l => l.IsActive)
            ))
            .ToListAsync();

        return Result.Success(categories);
    }
}