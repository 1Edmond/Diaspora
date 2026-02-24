using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands;

public record UpdateUserCommand(
    Guid UserId,
    string? FirstName,
    string? LastName,
    string? PhoneNumber
) : ICommand<Result>
{
    public Guid UserId { get; init; } = UserId;
}
