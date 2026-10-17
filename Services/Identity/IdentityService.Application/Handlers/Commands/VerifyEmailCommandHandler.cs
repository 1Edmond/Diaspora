using IdentityService.Application.Commands;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public static class VerifyEmailHandler
{
    public static async Task<Result> Handle(
        VerifyEmailCommand command,
        IRepository<User> repository,
        IUnitOfWork unitOfWork,
        ILogger<VerifyEmailCommand> logger)
    {
        var user = 
            (await repository.FindAsync(e => e.Email.Value.Equals(command.Email))
            ).FirstOrDefault();

        if (user == null)
        {
            return Result.Failure("User not found");
        }

        var result = user.VerifyEmail(command.Code);

        if (result.IsFailure)
        {
            return result;
        }

        await repository.UpdateAsync(user);
        await unitOfWork.SaveChangesAsync();


        logger.LogInformation("Email verified for user: {UserId}", user.Id);

        return Result.Success();
    }
}