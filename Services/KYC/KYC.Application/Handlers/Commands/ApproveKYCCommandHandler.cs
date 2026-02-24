using KYC.Application.Abstractions;
using KYC.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Handlers.Commands;

public class ApproveKYCCommandHandler(IKYCDbContext context)
{
    public async Task<Result> Handle(ApproveKYCCommand command)
    {
        var verification = await context.KYCVerifications
            .FirstOrDefaultAsync(v => v.Id == command.VerificationId);

        if (verification == null)
            return Result.Failure("Verification not found");

        if (!Enum.TryParse<KYCLevel>(command.Level, out var level))
            return Result.Failure("Invalid KYC level");

        var result = verification.Approve(command.ReviewerId, level);

        if (result.IsFailure)
            return result;

        await context.SaveChangesAsync();

        return Result.Success();
    }
}