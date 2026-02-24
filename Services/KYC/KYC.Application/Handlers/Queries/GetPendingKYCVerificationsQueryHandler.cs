using KYC.Application.Abstractions;
using KYC.Application.Dtos;
using KYC.Application.Mappers;
using KYC.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Handlers.Queries;

public class GetPendingKYCVerificationsQueryHandler
{
    private readonly IKYCDbContext _context;

    public GetPendingKYCVerificationsQueryHandler(IKYCDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedList<KYCVerificationDto>>> Handle(GetPendingKYCVerificationsQuery query)
    {
        var verificationsQuery = _context.KYCVerifications
            .Include(v => v.Documents)
            .Where(v => !v.IsDeleted && v.Status == KYCStatus.Pending);

        var totalCount = await verificationsQuery.CountAsync();

        var verifications = await verificationsQuery
            .OrderBy(v => v.SubmittedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = verifications.Select(KYCMapper.MapToDto).ToList();

        var pagedList = new PagedList<KYCVerificationDto>(dtos, totalCount, query.PageNumber, query.PageSize);

        return Result.Success(pagedList);
    }
}