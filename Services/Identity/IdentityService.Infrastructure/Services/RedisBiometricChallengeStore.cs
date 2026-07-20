using Elasticsearch.Net;
using IdentityService.Application.Abstractions;
using IdentityService.Application.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace IdentityService.Infrastructure.Services;

public class RedisBiometricChallengeStore : IBiometricChallengeStore
{
    private readonly IDatabase _database;

    public RedisBiometricChallengeStore(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<BiometricChallenge> CreateAsync(
        Guid userId,
        string deviceId,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        var nonceBytes = RandomNumberGenerator.GetBytes(32);
        var nonce = Convert.ToBase64String(nonceBytes);
        var expiresAt = DateTime.UtcNow.Add(ttl);

        var challenge = new BiometricChallenge(userId, deviceId, nonce, expiresAt);

        var key = BuildKey(userId, deviceId, nonce);

        var json = JsonSerializer.Serialize(challenge);

        await _database.StringSetAsync(
            key,
            json,
            expiry: ttl);

        return challenge;
    }

    public async Task<BiometricChallenge?> ConsumeAsync(
        Guid userId,
        string deviceId,
        string nonce,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(userId, deviceId, nonce);

        RedisValue value = await _database.StringGetDeleteAsync(key);

        if (value.IsNullOrEmpty)
            return null;
        byte[] bytes = value!;
        var challenge = JsonSerializer.Deserialize<BiometricChallenge>(bytes);

        if (challenge is null)
            return null;

        if (challenge.ExpiresAt < DateTime.UtcNow)
            return null;

        return challenge;
    }

    private static string BuildKey(Guid userId, string deviceId, string nonce) =>
        $"biometric-challenge:{userId}:{deviceId}:{nonce}";
}
