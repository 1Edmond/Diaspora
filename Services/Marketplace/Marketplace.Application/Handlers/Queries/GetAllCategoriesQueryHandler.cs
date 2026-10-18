using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Queries;

public class GetAllCategoriesQueryHandler(
    IApplicationDbContext _context,
    Shared.Application.Abstractions.ITranslationRepository<Marketplace.Domain.Entities.Category> _translationRepo,
    Shared.Application.Interfaces.ICurrentUserService _currentUserService)
{
    public async Task<Result<List<CategoryDto>>> Handle(GetAllCategoriesQuery query)
    {
        var rawCategories = await _context.Categories
            .Include(c => c.Policy)
            .Where(c => !query.ActiveOnly || c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.IconUrl,
                c.IsActive,
                c.DisplayOrder,
                c.AllowedProfileTypeIds,
                ActiveListingCount = c.Listings.Count(l => l.IsActive),
                Policy = c.Policy == null ? null : new CategoryPolicyDto(
                    c.Policy.MinPublishLevel,
                    c.Policy.MinBookLevel,
                    c.Policy.MaxWeightKg,
                    c.Policy.MaxValueAmount,
                    c.Policy.RequiresEscrow,
                    c.Policy.RequiresPreModeration,
                    c.Policy.DefaultCommissionRate
                )
            })
            .ToListAsync();

        if (query.ProfileTypeId.HasValue)
        {
            rawCategories = rawCategories
                .Where(c => c.AllowedProfileTypeIds == null 
                         || !c.AllowedProfileTypeIds.Any() 
                         || c.AllowedProfileTypeIds.Contains(query.ProfileTypeId.Value))
                .ToList();
        }

        var categoryIds = rawCategories.Select(c => c.Id).ToList();
        var translations = await _translationRepo.GetByOwnerIdsAsync(categoryIds);
        var preferredLang = _currentUserService.DataLanguage;

        var dtos = rawCategories.Select(c =>
        {
            var trans = translations.GetValueOrDefault(c.Id);
            var translatedName = Shared.Application.Translations.TranslationResolver.Resolve(c.Name, nameof(Marketplace.Domain.Entities.Category.Name), trans, preferredLang);
            var translatedDesc = Shared.Application.Translations.TranslationResolver.Resolve(c.Description ?? string.Empty, nameof(Marketplace.Domain.Entities.Category.Description), trans, preferredLang);

            return new CategoryDto(
                c.Id,
                translatedName,
                c.Slug,
                string.IsNullOrEmpty(translatedDesc) ? null : translatedDesc,
                c.IconUrl,
                c.IsActive,
                c.DisplayOrder,
                c.ActiveListingCount,
                c.Policy,
                c.AllowedProfileTypeIds
            );
        }).ToList();

        return Result.Success(dtos);
    }
}