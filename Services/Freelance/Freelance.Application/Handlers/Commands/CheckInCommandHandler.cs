using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Linq;

namespace Freelance.Application.Handlers.Commands;

public class CheckInCommandHandler(
    IJobApplicationRepository _applicationRepository,
    IJobCheckInRepository _checkInRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(CheckInCommand command)
    {
        var application = await _applicationRepository.GetByIdAsync(command.JobApplicationId);
        if (application is null)
            return Result.Failure<Guid>("Application not found.");

        if (application.WorkerId != command.WorkerId)
            return Result.Failure<Guid>("You are not the worker of this application.");

        if (application.Status != JobApplicationStatus.Accepted)
            return Result.Failure<Guid>("Can only check in to an accepted job application.");

        var existingCheckIns = await _checkInRepository.GetByJobApplicationAsync(command.JobApplicationId);
        if (existingCheckIns.Any(c => c.Status == CheckInStatus.CheckedIn))
            return Result.Failure<Guid>("Already checked in for this job application.");

        var pendingCheckIn = existingCheckIns.FirstOrDefault(c => c.Status == CheckInStatus.Pending);
        if (pendingCheckIn is not null)
        {
            var result = pendingCheckIn.CheckIn(command.Latitude, command.Longitude);
            if (result.IsFailure)
                return Result.Failure<Guid>(result.Error);

            await _checkInRepository.UpdateAsync(pendingCheckIn);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success(pendingCheckIn.Id);
        }

        var method = command.Latitude.HasValue && command.Longitude.HasValue
            ? CheckInMethod.GeoLocation
            : CheckInMethod.Manual;

        var checkIn = JobCheckIn.Create(
            command.JobApplicationId,
            method,
            command.Latitude,
            command.Longitude,
            command.Notes);

        var checkInResult = checkIn.CheckIn(command.Latitude, command.Longitude);
        if (checkInResult.IsFailure)
            return Result.Failure<Guid>(checkInResult.Error);

        await _checkInRepository.AddAsync(checkIn);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(checkIn.Id);
    }
}
