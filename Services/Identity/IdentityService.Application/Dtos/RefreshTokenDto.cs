using Shared.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Dtos;

public class RefreshTokenDto : EntityDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}