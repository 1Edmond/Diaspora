using IdentityService.Application.Dtos;
using IdentityService.Application.Queries;
using IdentityService.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Queries;

public static class GetUserQueryHandler
{
    public static async Task<Result<UserDto>> Handle(
        GetUserQuery query,
        IRepository<User> repository)
    {
        var user = await repository.GetByIdAsync(query.UserId);

        if (user == null)
        {
            return Result.Failure<UserDto>("User not found");
        }

        var dto = user.ToDto();

        return Result.Success(dto);
    }

}
