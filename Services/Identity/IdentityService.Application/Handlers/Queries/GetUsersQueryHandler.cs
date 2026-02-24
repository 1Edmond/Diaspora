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

public class GetUsersQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<UserDto>>> Handle(GetUsersQuery query)
    {
        var usersQuery = _context.Users.AsQueryable();

        if (!query.IncludeDeleted)
        {
            usersQuery = usersQuery.Where(u => !u.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            usersQuery = usersQuery.Where(u =>
                u.Email.Value.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                (u.FirstName != null && u.FirstName.ClearValue.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)) ||
                (u.LastName != null && u.LastName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)));
        }

        if (query.Status.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.Status == query.Status.Value);
        }

        if (query.EmailVerified.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.EmailVerified == query.EmailVerified.Value);
        }

        var totalCount = await usersQuery.CountAsync();

        var users = await usersQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => u.ToDto())
            .ToListAsync();

        var pagedList = new PagedList<UserDto>(users, totalCount, query.PageNumber, query.PageSize);

        return Result.Success(pagedList);
    }
}