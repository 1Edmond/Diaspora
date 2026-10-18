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
    public DbSet<ProfileType> ProfileTypes => Set<ProfileType>();
    public DbSet<Domain.Entities.Profile> InternalProfiles => Set<Domain.Entities.Profile>();
    public DbSet<ProfileTypeMobileConfig> MobileConfigs => Set<ProfileTypeMobileConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("profile");
    }
}
