using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Extensions;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Marketplace.Application.Queries.Admin;
using Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Queries;

public class AdminServiceRequestQueryHandler(
    IApplicationDbContext _context,
    IProfileDirectoryRepository _profileDirectory)
{
    public async Task<Result<PagedList<ServiceRequestDto>>> Handle(AdminServiceRequestQuery query)
    {
        var getAllQuery = new GetAllServiceRequestsQuery(
            query.PageNumber,
            query.PageSize,
            query.Status,
            query.PaymentStatus,
            query.ProviderId,
            query.RequesterId,
            query.DateFrom,
            query.DateTo,
            query.Search);

        var handler = new GetAllServiceRequestsQueryHandler(_context, _profileDirectory);
        return await handler.Handle(getAllQuery);
    }
}
