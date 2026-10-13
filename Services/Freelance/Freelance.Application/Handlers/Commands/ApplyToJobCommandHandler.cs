using Freelance.Application.Abstractions;
using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;

namespace Freelance.Application.Handlers.Commands;

public class ApplyToJobCommandHandler(
    IJobPostingRepository _postingRepository,
    IJobApplicationRepository _applicationRepository,
    IWorkerVerificationStatusRepository _verificationRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(ApplyToJobCommand command)
    {
        var recentCount = await _applicationRepository.CountCreatedSinceAsync(
            command.WorkerId, DateTime.UtcNow - RateLimitConstants.ApplicationCreationWindow);
        if (recentCount >= RateLimitConstants.MaxApplicationsPerWindow)
            return Result.Failure<Guid>(
                $"Trop de candidatures récentes. Limite : {RateLimitConstants.MaxApplicationsPerWindow} / {RateLimitConstants.ApplicationCreationWindow.TotalHours}h.");

        var posting = await _postingRepository.GetByIdAsync(command.JobPostingId);
        if (posting is null)
            return Result.Failure<Guid>("Job posting not found.");

        if (posting.Status != JobPostingStatus.Open)
            return Result.Failure<Guid>("This job posting is not open for applications.");

        if (posting.RegistrationDeadline.HasValue && posting.RegistrationDeadline.Value < DateTime.UtcNow)
            return Result.Failure<Guid>("Registration deadline has passed.");

        if (posting.AcceptedCount >= posting.Capacity)
            return Result.Failure<Guid>("This job posting has reached its capacity.");

        if (posting.RequiresKycVerification)
        {
            var verification = await _verificationRepository.GetByWorkerIdAsync(command.WorkerId);
            if (verification is null || !verification.IsVerified)
                return Result.Failure<Guid>("KYC verification is required for this job posting.");
        }

        var existing = await _applicationRepository.GetByWorkerAsync(command.WorkerId);
        if (existing.Any(a => a.JobPostingId == command.JobPostingId && a.Status != JobApplicationStatus.Withdrawn))
            return Result.Failure<Guid>("You have already applied to this job posting.");

        var application = JobApplication.Create(command.JobPostingId, command.WorkerId, command.Message, posting.EmployerId);

        await _applicationRepository.AddAsync(application);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(application.Id);
    }
}