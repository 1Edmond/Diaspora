using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Security.Abstractions;

public interface IEncryptionService
{
    Task<string> EncryptStringAsync(
        string plainText,
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default);

    Task<string> DecryptStringAsync(
        string cipherText,
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default);

    Task<byte[]> EncryptBytesAsync(
        byte[] plainBytes,
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default);

    Task<byte[]> DecryptBytesAsync(
        byte[] cipherBytes,
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default);
}
