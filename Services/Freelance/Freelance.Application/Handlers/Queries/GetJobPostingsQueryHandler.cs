using Freelance.Application.Abstractions;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Freelance.Application.Extensions;
using Freelance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Freelance.Application.Handlers.Queries;

public class GetJobPostingsQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<JobPostingSummaryDto>>> Handle(GetJobPostingsQuery query)
    {
        var q = _context.JobPostings
            .Include(p => p.Category)
            .AsQueryable();

        if (query.ActiveOnly)
            q = q.Where(p => !p.IsDeleted && p.Status != JobPostingStatus.Draft && p.Status != JobPostingStatus.Cancelled);

        if (query.CategoryId.HasValue)
            q = q.Where(p => p.CategoryId == query.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLower();
            q = q.Where(p => p.Title.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
        }

        if (query.PaymentType.HasValue)
            q = q.Where(p => p.PaymentType == query.PaymentType.Value);

        if (query.PaymentTiming.HasValue)
            q = q.Where(p => p.PaymentTiming == query.PaymentTiming.Value);

        if (query.Status.HasValue)
            q = q.Where(p => p.Status == query.Status.Value);

        if (query.EventStartFrom.HasValue)
            q = q.Where(p => p.EventStartAt >= query.EventStartFrom.Value);

        if (query.EventStartTo.HasValue)
            q = q.Where(p => p.EventStartAt <= query.EventStartTo.Value);

        if (!string.IsNullOrWhiteSpace(query.City))
            q = q.Where(p => p.City != null && p.City.ToLower() == query.City.ToLower());

        if (!string.IsNullOrWhiteSpace(query.Country))
            q = q.Where(p => p.Country != null && p.Country.ToLower() == query.Country.ToLower());

        if (query.IsRemote.HasValue)
            q = q.Where(p => p.IsRemote == query.IsRemote.Value);

        if (query.RequiredSkills != null && query.RequiredSkills.Any())
        {
            foreach (var skill in query.RequiredSkills)
            {
                var s = skill.ToLower();
                q = q.Where(p => p.RequiredSkills.Any(rs => rs.ToLower() == s));
            }
        }

        // Distance/remote filtering requires in-memory processing
        var needsInMemory = query.MaxDistanceKm.HasValue && query.UserLatitude.HasValue && query.UserLongitude.HasValue;

        if (!needsInMemory)
        {
            var totalCount = await q.CountAsync();

            var items = await q
                .OrderByDescending(p => p.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => p.MapToSummaryDto())
                .ToListAsync();

            return Result.Success(new PagedList<JobPostingSummaryDto>(items, totalCount, query.PageNumber, query.PageSize));
        }

        // In-memory path for distance filtering
        var candidates = await q.ToListAsync();
        var withDistance = candidates
            .Select(p => (
                Posting: p,
                Distance: (query.UserLatitude.HasValue && query.UserLongitude.HasValue && p.Latitude.HasValue && p.Longitude.HasValue)
                    ? GeoUtils.DistanceKm(query.UserLatitude.Value, query.UserLongitude.Value, p.Latitude.Value, p.Longitude.Value)
                    : (double?)null
            ))
            .Where(x => !query.MaxDistanceKm.HasValue || (x.Distance.HasValue && x.Distance.Value <= query.MaxDistanceKm.Value))
            .OrderBy(x => x.Distance ?? double.MaxValue)
            .ToList();

        var inMemoryTotal = withDistance.Count;
        var paged = withDistance
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => x.Posting.MapToSummaryDto(x.Distance))
            .ToList();

        return Result.Success(new PagedList<JobPostingSummaryDto>(paged, inMemoryTotal, query.PageNumber, query.PageSize));
    }
}







