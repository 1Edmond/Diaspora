using Document.Application.Interfaces;
using Document.Domain.Entities;
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

public sealed class DocumentTypeRepository(
    DbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<DocumentType>> logger,
    IDistributedCache cache
    )
    : Repository<DocumentType>(context,
         encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IDocumentTypeRepository
{
    private readonly DocumentDbContext _context = (DocumentDbContext)context;

    public async Task<DocumentType?> GetByCodeAsync(
        string code, CancellationToken cancellationToken = default)
        => await _context.DocumentTypes
            .FirstOrDefaultAsync(x => x.Code == code.ToUpperInvariant(), cancellationToken);

    public async Task<bool> ExistsByCodeAsync(
        string code, CancellationToken cancellationToken = default)
        => await _context.DocumentTypes
            .AnyAsync(x => x.Code == code.ToUpperInvariant(), cancellationToken);

    public async Task<IEnumerable<DocumentType>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
        => await _context.DocumentTypes
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
}
