using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands;

public record RegisterUserCommand(
    string Email,
    string Password,
    List<string> Role,
    string? FirstName,
    DateOnly DateOfBirth,
    string? LastName,
    string? PhoneNumber
    ) : ICommand<Result<Guid>>;
