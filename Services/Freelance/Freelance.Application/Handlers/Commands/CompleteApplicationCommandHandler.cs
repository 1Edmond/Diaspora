using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;

namespace Freelance.Application.Handlers.Commands;

public class CompleteApplicationCommandHandler(
    IJobPostingRepository _postingRepository,
    IJobApplicationRepository _applicationRepository,
    IFreelanceReputationRepository _reputationRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(CompleteApplicationCommand command)
    {
        var application = await _applicationRepository.GetByIdAsync(command.JobApplicationId);
        if (application is null)
            return Result.Failure("Application not found.");

        if (application.WorkerId != command.WorkerId)
            return Result.Failure("You are not the owner of this application.");

        var result = application.Complete();
        if (result.IsFailure)
            return Result.Failure(result.Error);

        // Mettre à jour la réputation du worker
        var workerRep = await _reputationRepository.GetBySubjectAsync(application.WorkerId, ReputationRole.Worker);
        if (workerRep is null)
        {
            workerRep = FreelanceReputation.CreateEmpty(application.WorkerId, ReputationRole.Worker);
            await _reputationRepository.AddAsync(workerRep);
        }
        workerRep.IncrementJobsCompleted();

        await _applicationRepository.UpdateAsync(application);
        await _reputationRepository.UpdateAsync(workerRep);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}