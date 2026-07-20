using Document.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Infrastructure.Persistence;

public sealed class DocumentDbContext(
    DbContextOptions<DocumentDbContext> options,
    IIntegrationEventMapper integrationEventMapper,
    IDateTimeProvider dateTimeProvider,
    ILogger<DocumentDbContext> logger,
    ICurrentUserService currentUserService)
    : BaseDbContext(options, integrationEventMapper, dateTimeProvider, logger, currentUserService)
{
    public DbSet<Domain.Entities.Document> Documents => Set<Domain.Entities.Document>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentDbContext).Assembly);
        modelBuilder.HasDefaultSchema("document");
    }
}