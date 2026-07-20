using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Extensions;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetServiceRequestByIdQueryHandler(IApplicationDbContext _context)
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

        return Result.Success(request.MapToDto());
    }
}