using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Infrastructure.Repositories;

public class ReportRepository(MarketplaceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<Report>> logger,
    IDistributedCache cache)
    : Repository<Report>(_context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache
        ), IReportRepository
{
}