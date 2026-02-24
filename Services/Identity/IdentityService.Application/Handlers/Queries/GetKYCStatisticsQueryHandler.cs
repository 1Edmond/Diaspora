using IdentityService.Application.Abstractions;
using IdentityService.Application.Dtos;
using IdentityService.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Queries;

public class GetKYCStatisticsQueryHandler
{
    private readonly IApplicationDbContext _context;

    public GetKYCStatisticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<KYCStatisticsDto>> Handle(GetKYCStatisticsQuery query)
    {
        var today = DateTime.UtcNow.Date;

        var totalUsers = await _context.Users
            .Where(u => !u.IsDeleted)
            .CountAsync();

        var usersWithNoKYC = await _context.Users
            .Where(u => !u.IsDeleted && u.KYCLevel == KYCLevel.None)
            .CountAsync();

        var usersWithBasicKYC = await _context.Users
            .Where(u => !u.IsDeleted && u.KYCLevel == KYCLevel.Basic)
            .CountAsync();

        var usersWithIntermediateKYC = await _context.Users
            .Where(u => !u.IsDeleted && u.KYCLevel == KYCLevel.Intermediate)
            .CountAsync();

        var usersWithAdvancedKYC = await _context.Users
            .Where(u => !u.IsDeleted && u.KYCLevel == KYCLevel.Advanced)
            .CountAsync();

        var kycSubmittedToday = await _context.Users
            .Where(u => !u.IsDeleted && u.KYCSubmittedAt.HasValue && u.KYCSubmittedAt.Value.Date == today)
            .CountAsync();

        var kycApprovedToday = await _context.Users
            .Where(u => !u.IsDeleted && u.KYCApprovedAt.HasValue && u.KYCApprovedAt.Value.Date == today)
            .CountAsync();

        var usersWithKYC = totalUsers - usersWithNoKYC;
        var verificationRate = totalUsers > 0 ? (decimal)usersWithKYC / totalUsers * 100 : 0;

        var statistics = new KYCStatisticsDto
        {
            TotalUsers = totalUsers,
            UsersWithNoKYC = usersWithNoKYC,
            UsersWithBasicKYC = usersWithBasicKYC,
            UsersWithIntermediateKYC = usersWithIntermediateKYC,
            UsersWithAdvancedKYC = usersWithAdvancedKYC,
            KYCSubmittedToday = kycSubmittedToday,
            KYCApprovedToday = kycApprovedToday,
            VerificationRate = Math.Round(verificationRate, 2)
        };

        return Result.Success(statistics);
    }
}
