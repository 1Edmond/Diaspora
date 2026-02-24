using KYC.Application.Abstractions;
using KYC.Application.Dtos;
using KYC.Application.Mappers;
using KYC.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Handlers.Queries;

public class GetKYCVerificationsQueryHandler
{
    private readonly IKYCDbContext _context;

    public GetKYCVerificationsQueryHandler(IKYCDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedList<KYCVerificationDto>>> Handle(GetKYCVerificationsQuery query)
    {
        var verificationsQuery = _context.KYCVerifications
            .Include(v => v.Documents)
            .Where(v => !v.IsDeleted)
            .AsQueryable();

        if (query.Status.HasValue)
            verificationsQuery = verificationsQuery.Where(v => v.Status == query.Status.Value);

        if (query.Level.HasValue)
            verificationsQuery = verificationsQuery.Where(v => v.Level == query.Level.Value);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            verificationsQuery = verificationsQuery.Where(v =>
                v.FirstName.ClearValue.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                v.LastName.ClearValue.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                v.Nationality.ClearValue.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase));
        }

        if (query.SubmittedFrom.HasValue)
            verificationsQuery = verificationsQuery.Where(v => v.SubmittedAt >= query.SubmittedFrom.Value);

        if (query.SubmittedTo.HasValue)
            verificationsQuery = verificationsQuery.Where(v => v.SubmittedAt <= query.SubmittedTo.Value);

        var totalCount = await verificationsQuery.CountAsync();

        var verifications = await verificationsQuery
            .OrderByDescending(v => v.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = verifications.Select(KYCMapper.MapToDto).ToList();

        var pagedList = new PagedList<KYCVerificationDto>(dtos, totalCount, query.PageNumber, query.PageSize);

        return Result.Success(pagedList);
    }
}
