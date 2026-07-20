using IdentityService.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Models;

public record LoginResponse(
    Guid UserId,
    UserDto User,
    List<string> Role,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
