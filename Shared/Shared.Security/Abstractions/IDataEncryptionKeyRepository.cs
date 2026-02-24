using Shared.Security.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Security.Abstractions;

public interface IDataEncryptionKeyRepository
{
    Task<DataEncryptionKey?> GetActiveAsync(
        string entityId,
        string entityType,
        CancellationToken cancellationToken);

    Task<DataEncryptionKey?> GetByIdAsync(
        Guid keyId,
        CancellationToken cancellationToken);

    Task DeactivateAllAsync(
        string entityId,
        string entityType,
        CancellationToken cancellationToken);

    Task SaveAsync(
        DataEncryptionKey key,
        CancellationToken cancellationToken);
}
