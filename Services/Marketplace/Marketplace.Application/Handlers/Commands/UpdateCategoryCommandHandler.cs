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

public class UpdateCategoryCommandHandler(
    ICategoryRepository _categoryRepository,
    ITranslationRepository<Category> _translationRepo,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(UpdateCategoryCommand command)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id);
        if (category is null)
            return Result.Failure("Category not found.");

        category.Update(command.Name, command.Description, command.IconUrl, command.DisplayOrder);
        await _categoryRepository.UpdateAsync(category);
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

        return Result.Success();
    }
}

