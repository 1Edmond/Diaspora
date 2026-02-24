using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public class RevokeAllRefreshTokensCommandHandler(IApplicationDbContext _context)
{
    public async Task<Result> Handle(RevokeAllRefreshTokensCommand command)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == command.UserId);

        if (user == null)
            return Result.Failure("User not found");

        foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
        {
            user.RevokeRefreshToken(token.Token, command.IpAddress);
        }

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}