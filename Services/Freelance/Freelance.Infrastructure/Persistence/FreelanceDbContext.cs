using Freelance.Application.Abstractions;
using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Persistence;
using Shared.Messaging.Events;
using System;

namespace Freelance.Infrastructure.Persistence;

public class FreelanceDbContext(
    DbContextOptions<FreelanceDbContext> options,
    IIntegrationEventMapper integrationEventMapper,
    IDateTimeProvider dateTimeProvider,
    ILogger<FreelanceDbContext> logger,
    ICurrentUserService currentUserService)
    : BaseDbContext(options, integrationEventMapper, dateTimeProvider, logger, currentUserService),
      IApplicationDbContext
{
    public DbSet<JobCategory> JobCategories => Set<JobCategory>();
    public DbSet<JobTemplate> JobTemplates => Set<JobTemplate>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<JobCheckIn> JobCheckIns => Set<JobCheckIn>();
    public DbSet<JobReview> JobReviews => Set<JobReview>();
    public DbSet<FreelanceReputation> FreelanceReputations => Set<FreelanceReputation>();
    public DbSet<WorkerJobPreference> WorkerJobPreferences => Set<WorkerJobPreference>();
    public DbSet<WorkerVerificationStatus> WorkerVerificationStatuses => Set<WorkerVerificationStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("freelance");
    }
}