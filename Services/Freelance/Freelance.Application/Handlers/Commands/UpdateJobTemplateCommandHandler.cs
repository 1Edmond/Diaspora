using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Handlers.Commands;

public class UpdateJobTemplateCommandHandler(
    IJobTemplateRepository _templateRepository,
    IJobCategoryRepository _categoryRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(UpdateJobTemplateCommand command)
    {
        var template = await _templateRepository.GetByIdAsync(command.Id);
        if (template is null)
            return Result.Failure("Job template not found.");

        if (template.EmployerId != command.RequestedBy)
            return Result.Failure("You are not the owner of this job template.");

        var category = await _categoryRepository.GetByIdAsync(command.CategoryId);
        if (category is null || !category.IsActive)
            return Result.Failure("Category not found or inactive.");

        var result = template.Update(
            command.Name, command.Description, command.CategoryId,
            command.Capacity, command.Amount, command.Currency,
            command.PaymentType, command.PaymentTiming, command.CheckInMethod,
            command.GeofenceRadiusMeters, command.RequiresKycVerification,
            command.RequiredSkills ?? Enumerable.Empty<string>(),
            command.RequiredDocuments ?? Enumerable.Empty<string>());

        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _templateRepository.UpdateAsync(template);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}