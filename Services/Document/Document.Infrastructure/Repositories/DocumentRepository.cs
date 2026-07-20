using Document.Application.Interfaces;
using Document.Domain.Enums;
using Document.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Infrastructure.Repositories;

public sealed class DocumentRepository(
    DbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<Domain.Entities.Document>> logger,
    IDistributedCache cache
    )
    : Repository<Domain.Entities.Document>(context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IDocumentRepository
{
    private readonly DocumentDbContext _context = (DocumentDbContext)context;

    public async Task<IEnumerable<Domain.Entities.Document>> GetByExternalProfileIdAsync(
        Guid externalProfileId, CancellationToken cancellationToken = default)
        => await _context.Documents
            .Include(x => x.DocumentType)
            .Where(x => x.ProfileId == externalProfileId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Domain.Entities.Document>> GetByExternalProfileIdAndTypeAsync(
        Guid externalProfileId, Guid documentTypeId, CancellationToken cancellationToken = default)
        => await _context.Documents
            .Include(x => x.DocumentType)
            .Where(x => x.ProfileId == externalProfileId
                     && x.DocumentTypeId == documentTypeId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Domain.Entities.Document>> GetExpiringDocumentsAsync(
        DateTime thresholdDate, CancellationToken cancellationToken = default)
        => await _context.Documents
            .Include(x => x.DocumentType)
            .Where(x => x.Status == DocumentStatus.Active
                     && x.ExpiresAt.HasValue
                     && x.ExpiresAt.Value <= thresholdDate
                     && x.ExpiresAt.Value > DateTime.UtcNow
                     && x.RenewalAlertSentAt == null)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Domain.Entities.Document>> GetExpiredActiveDocumentsAsync(
        CancellationToken cancellationToken = default)
        => await _context.Documents
            .Include(x => x.DocumentType)
            .Where(x => x.Status == DocumentStatus.Active
                     && x.ExpiresAt.HasValue
                     && x.ExpiresAt.Value <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Domain.Entities.Document>> GetByStatusAsync(
        DocumentStatus status, CancellationToken cancellationToken = default)
        => await _context.Documents
            .Include(x => x.DocumentType)
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
}
