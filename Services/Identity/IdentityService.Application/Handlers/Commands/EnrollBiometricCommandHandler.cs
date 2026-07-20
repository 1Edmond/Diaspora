using IdentityService.Application.Commands;
using IdentityService.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;


public class EnrollBiometricCommandHandler
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnrollBiometricCommandHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(EnrollBiometricCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure("User not found");

        var result = user.SetBiometriqueData(command.DeviceId, command.PublicKey);
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}