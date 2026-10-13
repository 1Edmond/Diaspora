using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Handlers.Commands;

public class CreateJobTemplateCommandHandler(
    IJobTemplateRepository _templateRepository,
    IJobCategoryRepository _categoryRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(CreateJobTemplateCommand command)
    {
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId);
        if (category is null || !category.IsActive)
            return Result.Failure<Guid>("Category not found or inactive.");

        var result = JobTemplate.Create(
            command.EmployerId, command.Name, command.Description, command.CategoryId,
            command.Capacity, command.Amount, command.Currency, command.PaymentType,
            command.PaymentTiming, command.CheckInMethod, command.GeofenceRadiusMeters,
            command.RequiresKycVerification, command.RequiredSkills ?? Enumerable.Empty<string>(),
            command.RequiredDocuments ?? Enumerable.Empty<string>());

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        await _templateRepository.AddAsync(result.Value);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(result.Value.Id);
    }
}