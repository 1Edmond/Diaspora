using IdentityService.Application.Commands;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public class ActivateUserCommandHandler
{
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<ActivateUserCommandHandler> _logger;

    public ActivateUserCommandHandler(
        IRepository<User> userRepository,
        ILogger<ActivateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(ActivateUserCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure("User not found");

        var result = user.Activate();
        if (result.IsFailure)
            return result;

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("User {UserId} activated", user.Id);
        return Result.Success();
    }
}