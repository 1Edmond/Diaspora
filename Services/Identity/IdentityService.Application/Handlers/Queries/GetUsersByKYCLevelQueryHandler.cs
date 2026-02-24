using IdentityService.Application.Abstractions;
using IdentityService.Application.Dtos;
using IdentityService.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Queries;

public class GetUsersByKYCLevelQueryHandler
{
    private readonly IApplicationDbContext _context;

    public GetUsersByKYCLevelQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedList<UserDto>>> Handle(GetUsersByKYCLevelQuery query)
    {
        var usersQuery = _context.Users
            .Where(u => !u.IsDeleted && u.KYCLevel == query.Level);

        var totalCount = await usersQuery.CountAsync();

        var users = await usersQuery
            .OrderByDescending(u => u.KYCApprovedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => u.ToDto())
            .ToListAsync();

        var pagedList = new PagedList<UserDto>(users, totalCount, query.PageNumber, query.PageSize);

        return Result.Success(pagedList);
    }
}
