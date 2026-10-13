using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;

namespace Freelance.Application.Handlers.Commands;

public class CloseJobPostingRegistrationCommandHandler(
    IJobPostingRepository _postingRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(CloseJobPostingRegistrationCommand command)
    {
        var posting = await _postingRepository.GetByIdAsync(command.Id);
        if (posting is null)
            return Result.Failure("Job posting not found.");

        if (posting.EmployerId != command.RequestedBy)
            return Result.Failure("You are not the owner of this job posting.");

        var result = posting.CloseRegistration();
        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _postingRepository.UpdateAsync(posting);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}