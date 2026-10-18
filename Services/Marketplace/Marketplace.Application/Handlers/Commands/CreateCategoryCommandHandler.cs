using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class CreateCategoryCommandHandler(
    ICategoryRepository _categoryRepository,
    ITranslationRepository<Category> _translationRepo,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand command)
    {
        var existing = await _categoryRepository.FirstOrDefaultAsync(c => c.Name == command.Name);
        if (existing is not null)
            return Result.Failure<Guid>("A category with this name already exists.");

        var category = Category.Create(command.Name, command.Description, command.IconUrl, command.DisplayOrder);
        if (command.AllowedProfileTypeIds is not null)
        {
            category.SetAllowedProfileTypes(command.AllowedProfileTypeIds);
        }

        await _categoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        if (command.Translations is not null && command.Translations.Count > 0)
        {
            foreach (var (cultureCode, fields) in command.Translations)
            {
                if (fields.Count > 0)
                {
                    await _translationRepo.UpsertAsync(category.Id, cultureCode, fields);
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }

        return Result.Success(category.Id);
    }
}