using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Linq;

namespace Freelance.Application.Handlers.Commands;

public class CheckOutCommandHandler(
    IJobApplicationRepository _applicationRepository,
    IJobCheckInRepository _checkInRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(CheckOutCommand command)
    {
        var application = await _applicationRepository.GetByIdAsync(command.JobApplicationId);
        if (application is null)
            return Result.Failure("Application not found.");

        if (application.WorkerId != command.WorkerId)
            return Result.Failure("You are not the worker of this application.");

        var checkIns = await _checkInRepository.GetByJobApplicationAsync(command.JobApplicationId);
        var checkIn = checkIns.FirstOrDefault(c => c.Status == CheckInStatus.CheckedIn);
        if (checkIn is null)
            return Result.Failure("No active check-in found.");

        var result = checkIn.CheckOut(command.Latitude, command.Longitude);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _checkInRepository.UpdateAsync(checkIn);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}