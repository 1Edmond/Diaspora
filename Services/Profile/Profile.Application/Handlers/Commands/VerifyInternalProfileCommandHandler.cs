using Profile.Application.Commands;
using Profile.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Handlers.Commands;

public sealed class VerifyInternalProfileCommandHandler
   
{
    private readonly IInternalProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyInternalProfileCommandHandler(
        IInternalProfileRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        VerifyInternalProfileCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.ProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure("Internal profile not found.");

        var result = profile.Verify(request.AdminId);
        if (result.IsFailure)
            return result;

        await _repository.UpdateAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}