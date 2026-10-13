using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;

namespace Freelance.Application.Handlers.Commands;

public class LinkApplicationChatThreadCommandHandler(
    IJobApplicationRepository _applicationRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(LinkApplicationChatThreadCommand command)
    {
        var application = await _applicationRepository.GetByIdAsync(command.JobApplicationId);
        if (application is null)
            return Result.Failure("Application not found.");

        if (application.WorkerId != command.RequestedBy && application.Posting.EmployerId != command.RequestedBy)
            return Result.Failure("You are not a participant of this application.");

        application.LinkChatThread(command.ChatThreadId);

        await _applicationRepository.UpdateAsync(application);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}