using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Freelance.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<JobCategory> JobCategories { get; }
    DbSet<JobTemplate> JobTemplates { get; }
    DbSet<JobPosting> JobPostings { get; }
    DbSet<JobApplication> JobApplications { get; }
    DbSet<JobCheckIn> JobCheckIns { get; }
    DbSet<JobReview> JobReviews { get; }
    DbSet<FreelanceReputation> FreelanceReputations { get; }
    DbSet<WorkerJobPreference> WorkerJobPreferences { get; }
    DbSet<WorkerVerificationStatus> WorkerVerificationStatuses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}