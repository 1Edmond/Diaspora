using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using Shared.Domain.Commons;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public class UpdateUserCommandHandler(IApplicationDbContext _context)
{

    public async Task<Result> Handle(UpdateUserCommand command)
    {
        var user = await _context.Users.FindAsync(command.UserId);

        if (user == null)
            return Result.Failure("User not found");

        if (user.IsDeleted)
            return Result.Failure("Cannot update deleted user");

        //PhoneNumber? phoneNumber = null;
        //if (!string.IsNullOrWhiteSpace(command.PhoneNumber))
        //{
        //    var phoneResult = PhoneNumber.Create(command.PhoneNumber);
        //    if (phoneResult.IsFailure)
        //        return Result.Failure(phoneResult.Error);

        //    phoneNumber = phoneResult.Value;
        //}

        user.Update(command.FirstName, command.LastName);

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}