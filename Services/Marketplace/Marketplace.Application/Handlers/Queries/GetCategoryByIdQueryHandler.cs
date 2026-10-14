using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetCategoryByIdQueryHandler(
    IApplicationDbContext _context,
    Shared.Application.Abstractions.ITranslationRepository<Marketplace.Domain.Entities.Category> _translationRepo,
    Shared.Application.Interfaces.ICurrentUserService _currentUserService)
{
    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery query)
    {
        var category = await _context.Categories
            .Where(c => c.Id == query.Id)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.IconUrl,
                c.IsActive,
                c.DisplayOrder,
                ActiveListingCount = c.Listings.Count(l => l.IsActive)
            })
            .FirstOrDefaultAsync();

        if (category is null)
            return Result.Failure<CategoryDto>("Category not found.");

        var translations = await _translationRepo.GetByOwnerIdAsync(category.Id);
        var preferredLang = _currentUserService.DataLanguage;

        var translatedName = Shared.Application.Translations.TranslationResolver.Resolve(
            category.Name, nameof(Marketplace.Domain.Entities.Category.Name), translations, preferredLang);
        var translatedDesc = Shared.Application.Translations.TranslationResolver.Resolve(
            category.Description ?? string.Empty, nameof(Marketplace.Domain.Entities.Category.Description), translations, preferredLang);

        var dto = new CategoryDto(
            category.Id,
            translatedName,
            category.Slug,
            string.IsNullOrEmpty(translatedDesc) ? null : translatedDesc,
            category.IconUrl,
            category.IsActive,
            category.DisplayOrder,
            category.ActiveListingCount
        );

        return Result.Success(dto);
    }
}
