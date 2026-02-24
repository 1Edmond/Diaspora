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

public sealed class CreateInternalProfileCommandHandler
{
    private readonly IInternalProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInternalProfileCommandHandler(
        IInternalProfileRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateInternalProfileResponse>> Handle(
        CreateInternalProfileCommand request,
        CancellationToken cancellationToken)
    {
        // One internal profile per user
        var exists = await _repository.ExistsByUserIdAsync(request.UserId, cancellationToken);
        if (exists)
            return Result.Failure<CreateInternalProfileResponse>(
                "An internal profile already exists for this user.");

        var profileResult = InternalProfile.Create(
            request.UserId,
            request.InternalProfileTypeId,
            request.PhoneNumber,
            request.FirstName,
            request.LastName,
            request.DateOfBirth);

        if (profileResult.IsFailure)
            return Result.Failure<CreateInternalProfileResponse>(profileResult.Error);

        await _repository.AddAsync(profileResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateInternalProfileResponse(profileResult.Value.Id));
    }
}