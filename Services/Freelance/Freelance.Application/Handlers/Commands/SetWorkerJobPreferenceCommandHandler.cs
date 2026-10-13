using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;

namespace Freelance.Application.Handlers.Commands;

public class SetWorkerJobPreferenceCommandHandler(
    IWorkerJobPreferenceRepository _preferenceRepository,
    IWorkerVerificationStatusRepository _verificationRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(SetWorkerJobPreferenceCommand command)
    {
        var verification = await _verificationRepository.GetByWorkerIdAsync(command.WorkerId);
        if (verification is null || !verification.IsVerified)
            return Result.Failure("Worker must be verified to set job preferences.");

        var preference = await _preferenceRepository.GetByWorkerIdAsync(command.WorkerId);
        if (preference is null)
        {
            preference = WorkerJobPreference.Create(command.WorkerId);
            await _preferenceRepository.AddAsync(preference);
        }

        if (command.CategoryIds != null)
            preference.SetCategories(command.CategoryIds);

        if (command.City != null || command.Country != null || command.Latitude != null || command.Longitude != null || command.RadiusKm != null)
            preference.SetLocation(command.City, command.Country, command.Latitude, command.Longitude, command.RadiusKm);

        if (command.OnlyRemote.HasValue)
            preference.SetRemotePreference(command.OnlyRemote.Value);

        if (command.EmailNotifications.HasValue)
            preference.SetEmailNotifications(command.EmailNotifications.Value);

        await _preferenceRepository.UpdateAsync(preference);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}