using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Dtos;

public static class UserMapper
{
    public static UserDto ToDto(this Domain.Entities.User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email.Value,
            Role = [..user.UserRoles.Select(r => r.Role.Name)],
            Status = user.Status.ToString(),
            
            EmailVerified = user.EmailVerified,
            KYCLevel = user.KYCLevel.ToString(),
            KYCSubmittedAt = user.KYCSubmittedAt,
            KYCApprovedAt = user.KYCApprovedAt,
            FirstName = user.FirstName?.ClearValue,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }
}
