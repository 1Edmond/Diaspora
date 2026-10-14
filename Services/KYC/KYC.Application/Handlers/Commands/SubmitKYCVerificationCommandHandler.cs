using KYC.Application.Abstractions;
using KYC.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Handlers.Commands;

public class SubmitKYCVerificationCommandHandler(
    IKYCDbContext context,
    IProfileDirectoryRepository? profileDirectoryRepository = null)
{
    public async Task<Result> Handle(SubmitKYCVerificationCommand command)
    {
        var verification = await context.KYCVerifications
            .Include(v => v.Documents)
            .FirstOrDefaultAsync(v => v.Id == command.VerificationId);

        if (verification == null)
            return Result.Failure("Verification not found");

        List<string>? requiredDocTypes = null;
        if (profileDirectoryRepository is not null)
        {
            var profile = await profileDirectoryRepository.GetByUserIdAsync(verification.UserId);
            if (profile is not null)
            {
                var requirements = await context.CountryKycRequirements
                    .Where(r => r.ProfileTypeId == profile.Id && r.IsActive && r.IsRequired)
                    .Select(r => r.DocumentTypeCode)
                    .ToListAsync();

                if (requirements.Count > 0)
                {
                    requiredDocTypes = requirements;
                }
            }
        }

        var result = verification.Submit(requiredDocTypes);

        if (result.IsFailure)
            return result;

        await context.SaveChangesAsync();

        return Result.Success();
    }
}