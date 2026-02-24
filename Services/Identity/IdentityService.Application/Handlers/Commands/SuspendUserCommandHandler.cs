using IdentityService.Application.Commands;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public class SuspendUserCommandHandler
{
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<SuspendUserCommandHandler> _logger;

    public SuspendUserCommandHandler(
        IRepository<User> userRepository,
        ILogger<SuspendUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(SuspendUserCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure("User not found");

        var result = user.Suspend(command.Reason);
        if (result.IsFailure)
            return result;

        await _userRepository.UpdateAsync(user);

        _logger.LogWarning("User {UserId} suspended. Reason: {Reason}", user.Id, command.Reason);
        return Result.Success();
    }
}