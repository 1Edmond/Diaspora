using Marketplace.Application.Abstractions;
using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Persistence.Translations;
using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Infrastructure.Persistence;

public class MarketplaceDbContext(
    DbContextOptions<MarketplaceDbContext> options,
    IIntegrationEventMapper integrationEventMapper,
    IDateTimeProvider dateTimeProvider,
    ILogger<MarketplaceDbContext> logger,
    ICurrentUserService currentUserService)
    : BaseDbContext(options, integrationEventMapper, dateTimeProvider, logger, currentUserService),
      IApplicationDbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ServiceListing> ServiceListings => Set<ServiceListing>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ProfileDirectoryEntry> ProfileDirectory => Set<ProfileDirectoryEntry>();
    public DbSet<CategoryPolicy> CategoryPolicies => Set<CategoryPolicy>();
    public DbSet<Dispute> Disputes => Set<Dispute>();
    public DbSet<ProfileTypePaymentPolicy> PaymentPolicies => Set<ProfileTypePaymentPolicy>();
    public DbSet<MarketplaceParameter> Parameters => Set<MarketplaceParameter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("marketplace");
        modelBuilder.ApplyTranslatableEntities();
    }
}
