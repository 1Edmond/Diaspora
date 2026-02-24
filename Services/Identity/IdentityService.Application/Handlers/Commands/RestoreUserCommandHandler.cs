using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public class RestoreUserCommandHandler(IApplicationDbContext _context)
{
    public async Task<Result> Handle(RestoreUserCommand command)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == command.UserId);

        if (user == null)
            return Result.Failure("User not found");

        if (!user.IsDeleted)
            return Result.Failure("User is not deleted");

        user.Restore();

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
