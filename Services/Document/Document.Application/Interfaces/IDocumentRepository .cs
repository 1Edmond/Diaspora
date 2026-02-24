using Document.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Interfaces;

public interface IDocumentRepository : IRepository<Domain.Entities.Document>
{
    Task<IEnumerable<Domain.Entities.Document>> GetByExternalProfileIdAsync(
        Guid externalProfileId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Domain.Entities.Document>> GetByExternalProfileIdAndTypeAsync(
        Guid externalProfileId, Guid documentTypeId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Domain.Entities.Document>> GetExpiringDocumentsAsync(
        DateTime thresholdDate, CancellationToken cancellationToken = default);

    Task<IEnumerable<Domain.Entities.Document>> GetExpiredActiveDocumentsAsync(
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Domain.Entities.Document>> GetByStatusAsync(
        DocumentStatus status, CancellationToken cancellationToken = default);
}