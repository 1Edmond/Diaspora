using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;

namespace Freelance.Application.Handlers.Commands;

public class CreateJobReviewCommandHandler(
    IJobApplicationRepository _applicationRepository,
    IJobReviewRepository _reviewRepository,
    IFreelanceReputationRepository _reputationRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(CreateJobReviewCommand command)
    {
        var application = await _applicationRepository.GetByIdAsync(command.JobApplicationId);
        if (application is null)
            return Result.Failure<Guid>("Application not found.");

        if (application.Status != JobApplicationStatus.Completed)
            return Result.Failure<Guid>("Can only review completed jobs.");

        // Vérifier que le rater fait partie de l'application
        if (command.RaterId != application.WorkerId && command.RaterId != application.Posting.EmployerId)
            return Result.Failure<Guid>("You are not a participant of this job.");

        // Déterminer le subject
        var subjectId = command.RaterId == application.WorkerId
            ? application.Posting.EmployerId
            : application.WorkerId;

        var alreadyReviewed = await _reviewRepository.HasReviewedAsync(command.JobApplicationId, command.RaterId);
        if (alreadyReviewed)
            return Result.Failure<Guid>("You have already reviewed this job.");

        var reviewResult = JobReview.Create(
            command.JobApplicationId, command.RaterId, subjectId, command.RaterRole,
            command.Rating, command.Punctuality, command.Quality, command.Communication, command.Comment);

        if (reviewResult.IsFailure)
            return Result.Failure<Guid>(reviewResult.Error);

        var review = reviewResult.Value;
        await _reviewRepository.AddAsync(review);

        // Mettre à jour la réputation du subject
        var subjectRole = command.RaterRole == RaterRole.Worker ? ReputationRole.Employer : ReputationRole.Worker;
        var reputation = await _reputationRepository.GetBySubjectAsync(subjectId, subjectRole);
        if (reputation is null)
        {
            reputation = FreelanceReputation.CreateEmpty(subjectId, subjectRole);
            await _reputationRepository.AddAsync(reputation);
        }
        reputation.ApplyNewReview(command.Rating, command.Punctuality, command.Quality, command.Communication);

        await _reputationRepository.UpdateAsync(reputation);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(review.Id);
    }
}