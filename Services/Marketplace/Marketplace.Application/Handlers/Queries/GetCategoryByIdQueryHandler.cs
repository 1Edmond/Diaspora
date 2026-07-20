using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetCategoryByIdQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery query)
    {
        var category = await _context.Categories
            .Where(c => c.Id == query.Id)
            .Select(c => new CategoryDto(
                c.Id, c.Name, c.Slug, c.Description, c.IconUrl,
                c.IsActive, c.DisplayOrder, c.Listings.Count(l => l.IsActive)
            ))
            .FirstOrDefaultAsync();

        if (category is null)
            return Result.Failure<CategoryDto>("Category not found.");

        return Result.Success(category);
    }
}
