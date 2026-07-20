using IdentityService.Application.Models;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands;

public record LoginCommand(
    string Email,
    string Password,
    string? IpAddress) : ICommand<Result<LoginResponse>>;
