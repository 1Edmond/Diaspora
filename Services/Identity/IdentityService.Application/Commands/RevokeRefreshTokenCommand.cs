using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands;

public record RevokeRefreshTokenCommand(
    Guid UserId,
    string Token,
    string IpAddress) : ICommand<Result>;