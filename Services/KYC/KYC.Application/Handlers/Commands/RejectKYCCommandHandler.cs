using KYC.Application.Abstractions;
using KYC.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Handlers.Commands;

public class RejectKYCCommandHandler(IKYCDbContext context)
{
    public async Task<Result> Handle(RejectKYCCommand command)
    {
        var verification = await context.KYCVerifications
            .FirstOrDefaultAsync(v => v.Id == command.VerificationId);

        if (verification == null)
            return Result.Failure("Verification not found");

        var result = verification.Reject(command.ReviewerId, command.Reason);

        if (result.IsFailure)
            return result;

        await context.SaveChangesAsync();

        return Result.Success();
    }
}