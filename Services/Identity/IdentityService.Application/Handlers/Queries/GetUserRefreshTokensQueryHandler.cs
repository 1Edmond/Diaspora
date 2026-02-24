using IdentityService.Application.Abstractions;
using IdentityService.Application.Dtos;
using IdentityService.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Queries;

public class GetUserRefreshTokensQueryHandler(IApplicationDbContext _context)
{

    public async Task<Result<List<RefreshTokenDto>>> Handle(GetUserRefreshTokensQuery query)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == query.UserId);

        if (user == null)
            return Result.Failure<List<RefreshTokenDto>>("User not found");

        var tokens = user.RefreshTokens
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new RefreshTokenDto
            {
                Token = t.Token,
                Expires = t.ExpiresAt,
                CreatedAt = t.CreatedAt,
                CreatedByIp = t.CreatedByIp,
                RevokedAt = t.RevokedAt,
                RevokedByIp = t.RevokedByIp,
                ReplacedByToken = t.ReplacedByToken
            })
            .ToList();

        return Result.Success(tokens);
    }
}
