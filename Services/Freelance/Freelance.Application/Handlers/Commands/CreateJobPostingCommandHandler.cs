using Freelance.Application.Abstractions;
using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Freelance.Application.Handlers.Commands;

public class CreateJobPostingCommandHandler(
    IJobPostingRepository _postingRepository,
    IJobCategoryRepository _categoryRepository,
    IJobTemplateRepository _templateRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(CreateJobPostingCommand command)
    {
        var recentCount = await _postingRepository.CountCreatedSinceAsync(
            command.EmployerId, DateTime.UtcNow - RateLimitConstants.JobPostingCreationWindow);
        if (recentCount >= RateLimitConstants.MaxJobPostingsPerWindow)
            return Result.Failure<Guid>(
                $"Trop de publications récentes. Limite : {RateLimitConstants.MaxJobPostingsPerWindow} offres / {RateLimitConstants.JobPostingCreationWindow.TotalHours}h.");

        var category = await _categoryRepository.GetByIdAsync(command.CategoryId);
        if (category is null || !category.IsActive)
            return Result.Failure<Guid>("Category not found or inactive.");

        if (command.TemplateId.HasValue)
        {
            var template = await _templateRepository.GetByIdAsync(command.TemplateId.Value);
            if (template is null || template.EmployerId != command.EmployerId)
                return Result.Failure<Guid>("Template not found or access denied.");
        }

        var result = JobPosting.Create(
            command.EmployerId, command.TemplateId, command.CategoryId,
            command.Title, command.Description, command.Capacity, command.Amount,
            command.Currency, command.PaymentType, command.PaymentTiming,
            command.CheckInMethod, command.GeofenceRadiusMeters, command.RequiresKycVerification,
            command.EventStartAt, command.EventEndAt, command.City, command.Country,
            command.Latitude, command.Longitude, command.IsRemote,
            command.RequiredSkills ?? Enumerable.Empty<string>(),
            command.RequiredDocuments ?? Enumerable.Empty<string>(),
            command.RegistrationDeadline);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        await _postingRepository.AddAsync(result.Value);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(result.Value.Id);
    }
}