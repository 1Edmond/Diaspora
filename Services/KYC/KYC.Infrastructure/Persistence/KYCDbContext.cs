using KYC.Application.Abstractions;
using KYC.Domain.Entities;
using KYC.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Infrastructure.Persistence;

public class KYCDbContext(DbContextOptions<KYCDbContext> options,
        IIntegrationEventMapper _integrationEventMapper,
        IDateTimeProvider _dateTimeProvider,
        ICurrentUserService currentUserService,
        ILogger<KYCDbContext> logger)
        : BaseDbContext(options: options,
            _integrationEventMapper: _integrationEventMapper,
            _dateTimeProvider: _dateTimeProvider,
            _currentUserService: currentUserService,
            _logger: logger), IKYCDbContext
{

    public DbSet<KYCVerification> KYCVerifications => Set<KYCVerification>();

    public DbSet<KYCDocument> KYCDocuments => Set<KYCDocument>();

    public DbSet<ProfileDirectoryEntry> ProfileDirectoryEntries => Set<ProfileDirectoryEntry>();

    public DbSet<CountryKycRequirement> CountryKycRequirements => Set<CountryKycRequirement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new KYCVerificationConfiguration());
        modelBuilder.ApplyConfiguration(new KYCDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new ProfileDirectoryEntryConfiguration());
        modelBuilder.ApplyConfiguration(new CountryKycRequirementConfiguration());
        modelBuilder.HasDefaultSchema("kyc");
    }
}

