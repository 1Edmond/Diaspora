using KYC.Application.Abstractions;
using KYC.Application.Dtos;
using KYC.Application.Mappers;
using KYC.Application.Queries;
using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Handlers.Queries;

public class GetKYCVerificationQueryHandler(IKYCDbContext context)
{
    public async Task<Result<KYCVerificationDto>> Handle(GetKYCVerificationQuery query)
    {
        var verification = await context.KYCVerifications
            .Include(v => v.Documents)
            .FirstOrDefaultAsync(v => v.Id == query.VerificationId);

        if (verification == null)
            return Result.Failure<KYCVerificationDto>("Verification not found");

        var dto = KYCMapper.MapToDto(verification);

        return Result.Success(dto);
    }


}