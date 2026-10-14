using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Extensions;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetServiceRequestByIdQueryHandler(
    IApplicationDbContext _context,
    IProfileDirectoryRepository _profileDirectory)
{
    public async Task<Result<ServiceRequestDto>> Handle(GetServiceRequestByIdQuery query)
    {
        var request = await _context.ServiceRequests
            .Include(r => r.Listing)
            .FirstOrDefaultAsync(r => r.Id == query.Id);

        if (request is null)
            return Result.Failure<ServiceRequestDto>("Request not found.");

        if (request.RequesterId != query.RequestedBy && request.ProviderId != query.RequestedBy)
            return Result.Failure<ServiceRequestDto>("Access denied.");

        var profiles = await _profileDirectory.GetByUserIdsAsync(new[] { request.RequesterId, request.ProviderId });
        profiles.TryGetValue(request.RequesterId, out var requesterProfile);
        profiles.TryGetValue(request.ProviderId, out var providerProfile);

        return Result.Success(request.MapToDto(requesterProfile, providerProfile));
    }
}