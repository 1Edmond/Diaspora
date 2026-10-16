using IdentityService.Application.Commands;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public class ChangePasswordCommandHandler
{
    private readonly IRepository<User> _userRepository;
    private readonly IHashingService _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IRepository<User> userRepository,
        IHashingService passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangePasswordCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure("User not found");

        if (!_passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            return Result.Failure("Current password is incorrect");

       
        var newPasswordHash = _passwordHasher.HashPassword(command.NewPassword);

        var result = user.ChangePassword(newPasswordHash);
        if (result.IsFailure)
            return result;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Password changed for user {UserId}", user.Id);
        return Result.Success();
    }
}