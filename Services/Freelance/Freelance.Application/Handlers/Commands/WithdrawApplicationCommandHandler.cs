using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;

namespace Freelance.Application.Handlers.Commands;

public class WithdrawApplicationCommandHandler(
    IJobPostingRepository _postingRepository,
    IJobApplicationRepository _applicationRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(WithdrawApplicationCommand command)
    {
        var application = await _applicationRepository.GetByIdAsync(command.JobApplicationId);
        if (application is null)
            return Result.Failure("Application not found.");

        if (application.WorkerId != command.WorkerId)
            return Result.Failure("You are not the owner of this application.");

        var posting = await _postingRepository.GetByIdAsync(application.JobPostingId);
        if (posting is null)
            return Result.Failure("Job posting not found.");

        var result = application.Withdraw();
        if (result.IsFailure)
            return Result.Failure(result.Error);

        if (application.Status == JobApplicationStatus.Accepted)
            posting.NotifyAcceptedApplicationReversed();

        await _applicationRepository.UpdateAsync(application);
        if (application.Status == JobApplicationStatus.Accepted)
            await _postingRepository.UpdateAsync(posting);

        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}