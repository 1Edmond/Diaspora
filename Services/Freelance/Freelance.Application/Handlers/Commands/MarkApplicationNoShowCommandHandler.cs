using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;

namespace Freelance.Application.Handlers.Commands;

public class MarkApplicationNoShowCommandHandler(
    IJobPostingRepository _postingRepository,
    IJobApplicationRepository _applicationRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(MarkApplicationNoShowCommand command)
    {
        var application = await _applicationRepository.GetByIdAsync(command.JobApplicationId);
        if (application is null)
            return Result.Failure("Application not found.");

        var posting = await _postingRepository.GetByIdAsync(application.JobPostingId);
        if (posting is null)
            return Result.Failure("Job posting not found.");

        if (posting.EmployerId != command.DecidedBy)
            return Result.Failure("You are not the employer of this job posting.");

        var result = application.MarkNoShow(command.DecidedBy);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        posting.NotifyAcceptedApplicationReversed();

        await _applicationRepository.UpdateAsync(application);
        await _postingRepository.UpdateAsync(posting);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}