using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public class RevokeRefreshTokensCommandHandler(IApplicationDbContext _context)
{
    public async Task<Result> Handle(RevokeRefreshTokenCommand command)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == command.UserId);

        if (user == null)
            return Result.Failure("User not found");

        var token = user.RefreshTokens.Where(t => t.Id == command.UserId).FirstOrDefault();

        if (token is null)
            return Result.Failure("Token not found");

        user.RevokeRefreshToken(token.Token, command.IpAddress);

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}