using Freelance.Application.Commands;
using Freelance.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;

namespace Freelance.Application.Handlers.Commands;

public class DeleteJobTemplateCommandHandler(
    IJobTemplateRepository _templateRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(DeleteJobTemplateCommand command)
    {
        var template = await _templateRepository.GetByIdAsync(command.Id);
        if (template is null)
            return Result.Failure("Job template not found.");

        if (template.EmployerId != command.RequestedBy)
            return Result.Failure("You are not the owner of this job template.");

        await _templateRepository.DeleteAsync(template);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}