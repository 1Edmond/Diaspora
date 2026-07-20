using IdentityService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Abstractions;

public interface IBiometricChallengeStore
{
    Task<BiometricChallenge> CreateAsync(
        Guid userId,
        string deviceId,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task<BiometricChallenge?> ConsumeAsync(
        Guid userId,
        string deviceId,
        string nonce,
        CancellationToken cancellationToken = default);
}