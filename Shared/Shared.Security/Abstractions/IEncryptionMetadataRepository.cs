using Shared.Security.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Security.Abstractions;

public interface IEncryptionMetadataRepository
{
    Task SaveAsync(
        EncryptionMetadataRecord metadata,
        CancellationToken cancellationToken = default);

    Task<EncryptionMetadataRecord?> GetByPropertyAsync(
        string entityType,
        string entityId,
        string propertyName,
        CancellationToken cancellationToken = default);
}