using Profile.Application.Commands;
using Profile.Application.Interfaces;
using Profile.Application.Responses.Commands;
using Profile.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Handlers.Commands;

public sealed class CreateProfileCommandHandler
{
    private readonly IProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProfileCommandHandler(
        IProfileRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateProfileResponse>> Handle(
        CreateProfileCommand request,
        CancellationToken cancellationToken)
    {
        // One internal profile per user
        var exists = await _repository.ExistsByUserIdAsync(request.UserId, cancellationToken);
        if (exists)
            return Result.Failure<CreateProfileResponse>(
                "A profile already exists for this user.");

        var profileResult = Domain.Entities.Profile.Create(
            request.UserId,
            request.ProfileTypeId,
            request.PhoneNumber,
            request.FirstName,
            request.LastName,
            request.DateOfBirth);

        if (profileResult.IsFailure)
            return Result.Failure<CreateProfileResponse>(profileResult.Error);

        await _repository.AddAsync(profileResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateProfileResponse(profileResult.Value.Id));
    }
}