using Microsoft.Extensions.Logging;
using Shared.Security.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Security.Services;

public class EncryptionService : IEncryptionService
{
    private readonly IKeyManagementService _keyManagement;
    private readonly ILogger<EncryptionService> _logger;

    public EncryptionService(IKeyManagementService keyManagement, ILogger<EncryptionService> logger)
    {
        _keyManagement = keyManagement;
        _logger = logger;
    }

    public async Task<string> EncryptStringAsync(
        string plainText,
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = await EncryptBytesAsync(plainBytes, entityId, entityType, cancellationToken);
        return Convert.ToBase64String(encryptedBytes);
    }

    public async Task<string> DecryptStringAsync(
        string cipherText,
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        var cipherBytes = Convert.FromBase64String(cipherText);
        var decryptedBytes = await DecryptBytesAsync(cipherBytes, entityId, entityType, cancellationToken);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public async Task<byte[]> EncryptBytesAsync(
        byte[] plainBytes,
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        var (keyId, dek) = await _keyManagement.GetOrCreateDataEncryptionKeyAsync(
            entityId,
            entityType,
            cancellationToken);

        try
        {
            using var aes = new AesGcm(dek, AesGcm.TagByteSizes.MaxSize);

            var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
            var tag = new byte[AesGcm.TagByteSizes.MaxSize];
            var cipherText = new byte[plainBytes.Length];

            RandomNumberGenerator.Fill(nonce);
            aes.Encrypt(nonce, plainBytes, cipherText, tag);

            return nonce.Concat(tag).Concat(cipherText).ToArray();
        }
        finally
        {
            Array.Clear(dek, 0, dek.Length);
        }
    }

    public async Task<byte[]> DecryptBytesAsync(
        byte[] cipherBytes,
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        var dek = await _keyManagement.GetDataEncryptionKeyAsync(
            entityId,
            entityType,
            cancellationToken);

        if (dek == null)
        {
            throw new InvalidOperationException(
                $"No DEK found for entity {entityType}:{entityId}");
        }

        try
        {
            var nonce = cipherBytes.Take(AesGcm.NonceByteSizes.MaxSize).ToArray();
            var tag = cipherBytes
                .Skip(AesGcm.NonceByteSizes.MaxSize)
                .Take(AesGcm.TagByteSizes.MaxSize)
                .ToArray();
            var cipherText = cipherBytes
                .Skip(AesGcm.NonceByteSizes.MaxSize + AesGcm.TagByteSizes.MaxSize)
                .ToArray();

            var plainText = new byte[cipherText.Length];

            using var aes = new AesGcm(dek, AesGcm.TagByteSizes.MaxSize);
            aes.Decrypt(nonce, cipherText, tag, plainText);

            return plainText;
        }
        finally
        {
            Array.Clear(dek, 0, dek.Length);
        }
    }
}