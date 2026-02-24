using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Domain.Entities;

public class RefreshToken : AggregateRoot
{
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    public static RefreshToken Create(string token, string ipAddress, DateTime expires)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            ExpiresAt = expires,
            CreatedByIp = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Revoke(string? ipAddress = null, string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = ipAddress;
        ReplacedByToken = replacedByToken;
    }
}
