using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;

namespace Freelance.Application.Handlers.Commands;

public class RemoveWorkerJobPreferenceCommandHandler(
    IWorkerJobPreferenceRepository _preferenceRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(RemoveWorkerJobPreferenceCommand command)
    {
        var preference = await _preferenceRepository.GetByWorkerIdAsync(command.WorkerId);
        if (preference is null)
            return Result.Success();

        await _preferenceRepository.DeleteAsync(preference);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}