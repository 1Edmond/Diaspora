using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Profile.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Infrastructure.Persistence;

public sealed class ProfileDbContext(
    DbContextOptions<ProfileDbContext> options,
    IIntegrationEventMapper integrationEventMapper,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProfileDbContext> logger,
    ICurrentUserService currentUserService)
    : BaseDbContext(options, integrationEventMapper, dateTimeProvider, logger, currentUserService)
{
    public DbSet<InternalProfileType> InternalProfileTypes => Set<InternalProfileType>();
    public DbSet<InternalProfile> InternalProfiles => Set<InternalProfile>();
    public DbSet<ExternalProfile> ExternalProfiles => Set<ExternalProfile>();
    public DbSet<ExternalProfileType> ExternalProfileTypes => Set<ExternalProfileType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Schéma dédié — pas de collision avec les autres services
        modelBuilder.HasDefaultSchema("profile");
    }
}
