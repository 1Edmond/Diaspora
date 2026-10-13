using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;

namespace Freelance.Application.Handlers.Commands;

public class UpdateJobPostingCommandHandler(
    IJobPostingRepository _postingRepository,
    IJobCategoryRepository _categoryRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(UpdateJobPostingCommand command)
    {
        var posting = await _postingRepository.GetByIdAsync(command.Id);
        if (posting is null)
            return Result.Failure("Job posting not found.");

        if (posting.EmployerId != command.RequestedBy)
            return Result.Failure("You are not the owner of this job posting.");

        var category = await _categoryRepository.GetByIdAsync(command.CategoryId);
        if (category is null || !category.IsActive)
            return Result.Failure("Category not found or inactive.");

        var result = posting.Update(
            command.Title, command.Description, command.CategoryId,
            command.Capacity, command.Amount, command.Currency,
            command.PaymentType, command.PaymentTiming, command.CheckInMethod,
            command.GeofenceRadiusMeters, command.RequiresKycVerification,
            command.EventStartAt, command.EventEndAt, command.City, command.Country,
            command.Latitude, command.Longitude, command.IsRemote,
            command.RequiredSkills ?? Enumerable.Empty<string>(),
            command.RequiredDocuments ?? Enumerable.Empty<string>(),
            command.RegistrationDeadline);

        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _postingRepository.UpdateAsync(posting);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}