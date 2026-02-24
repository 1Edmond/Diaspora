using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Models;

public record LoginResponse(
    Guid UserId,
    string Email,
    List<string> Role,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
