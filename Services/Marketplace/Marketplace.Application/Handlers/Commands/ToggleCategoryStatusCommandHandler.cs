using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class ToggleCategoryStatusCommandHandler(ICategoryRepository _categoryRepository, IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(ToggleCategoryStatusCommand command)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id);
        if (category is null)
            return Result.Failure("Category not found.");

        if (command.IsActive) category.Activate();
        else category.Deactivate();

        await _categoryRepository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}