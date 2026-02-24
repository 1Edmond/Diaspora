using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public class DeleteUserCommandHandler(IApplicationDbContext _context)
{

    public async Task<Result> Handle(DeleteUserCommand command)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == command.UserId);

        if (user == null)
            return Result.Failure("User not found");

        if (user.IsDeleted)
            return Result.Failure("User already deleted");

        user.Delete(command.DeletedBy);

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
