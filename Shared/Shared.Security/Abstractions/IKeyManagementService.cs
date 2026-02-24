using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Security.Abstractions;

public interface IKeyManagementService
{
    Task<(Guid KeyId, byte[] DataKey)> GetOrCreateDataEncryptionKeyAsync(
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default);

    Task<byte[]?> GetDataEncryptionKeyAsync(
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default);

    Task<Guid?> GetActiveKeyIdAsync(
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default);
}
