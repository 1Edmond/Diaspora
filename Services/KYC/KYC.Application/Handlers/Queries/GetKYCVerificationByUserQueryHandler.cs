using KYC.Application.Abstractions;
using KYC.Application.Dtos;
using KYC.Application.Mappers;
using KYC.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Handlers.Queries;

public class GetKYCVerificationByUserQueryHandler
{
    private readonly IKYCDbContext _context;

    public GetKYCVerificationByUserQueryHandler(IKYCDbContext context)
    {
        _context = context;
    }

    public async Task<Result<KYCVerificationDto>> Handle(GetKYCVerificationByUserQuery query)
    {
        var verification = await _context.KYCVerifications
            .Include(v => v.Documents)
            .Where(v => v.UserId == query.UserId && !v.IsDeleted)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync();

        if (verification == null)
            return Result.Failure<KYCVerificationDto>("No KYC verification found for this user");

        var dto = KYCMapper.MapToDto(verification);

        return Result.Success(dto);
    }
}
