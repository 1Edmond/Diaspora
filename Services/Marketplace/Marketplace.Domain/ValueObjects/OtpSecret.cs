using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Marketplace.Domain.ValueObjects;

public class OtpSecret : ValueObject
{
    public const int MaxFailedAttempts = 5;

    public string Hash { get; private set; }
    public string Salt { get; private set; }
    public int FailedAttempts { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    public bool IsLockedOut => FailedAttempts >= MaxFailedAttempts;

    private OtpSecret(string hash, string salt, int failedAttempts, DateTime? expiresAt)
    {
        Hash = hash;
        Salt = salt;
        FailedAttempts = failedAttempts;
        ExpiresAt = expiresAt;
    }

    public static OtpSecret Generate(out string rawCode, TimeSpan? validity = null)
    {
        rawCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);
        var hash = ComputeHash(rawCode, salt);
        var expiresAt = DateTime.UtcNow.Add(validity ?? TimeSpan.FromDays(30));

        return new OtpSecret(hash, salt, 0, expiresAt);
    }

    public static OtpSecret Create(string rawCode, TimeSpan? validity = null)
    {
        if (string.IsNullOrWhiteSpace(rawCode))
            throw new ArgumentException("Raw OTP code cannot be empty.", nameof(rawCode));

        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);
        var hash = ComputeHash(rawCode.Trim(), salt);
        var expiresAt = DateTime.UtcNow.Add(validity ?? TimeSpan.FromDays(30));

        return new OtpSecret(hash, salt, 0, expiresAt);
    }

    public bool Verify(string candidateCode)
    {
        if (IsLockedOut || IsExpired)
            return false;

        if (string.IsNullOrWhiteSpace(candidateCode))
        {
            FailedAttempts++;
            return false;
        }

        var candidateHash = ComputeHash(candidateCode.Trim(), Salt);

        var hashBytes = Convert.FromBase64String(Hash);
        var candidateBytes = Convert.FromBase64String(candidateHash);

        var isMatch = CryptographicOperations.FixedTimeEquals(hashBytes, candidateBytes);

        if (isMatch)
        {
            return true;
        }

        FailedAttempts++;
        return false;
    }

    public string ToStorageString()
    {
        var expiresTick = ExpiresAt.HasValue ? ExpiresAt.Value.Ticks.ToString() : string.Empty;
        return $"V1:{Hash}:{Salt}:{FailedAttempts}:{expiresTick}";
    }

    public static OtpSecret FromStorage(string storageString)
    {
        if (string.IsNullOrWhiteSpace(storageString))
            throw new ArgumentException("Storage string cannot be empty.", nameof(storageString));

        var parts = storageString.Split(':');
        if (parts.Length == 5 && parts[0] == "V1")
        {
            var hash = parts[1];
            var salt = parts[2];
            var failedAttempts = int.TryParse(parts[3], out var attempts) ? attempts : 0;
            DateTime? expiresAt = long.TryParse(parts[4], out var ticks) && ticks > 0
                ? new DateTime(ticks, DateTimeKind.Utc)
                : null;

            return new OtpSecret(hash, salt, failedAttempts, expiresAt);
        }

        // Fallback pour format brut
        return new OtpSecret(storageString, string.Empty, 0, null);
    }

    private static string ComputeHash(string code, string salt)
    {
        var combined = $"{salt}:{code}";
        var bytes = Encoding.UTF8.GetBytes(combined);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Hash;
        yield return Salt;
        yield return FailedAttempts;
        if (ExpiresAt.HasValue) yield return ExpiresAt.Value;
    }
}
