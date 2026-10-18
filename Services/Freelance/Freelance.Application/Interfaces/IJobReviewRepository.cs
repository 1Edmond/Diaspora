using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Interfaces;

public interface IJobReviewRepository : IRepository<JobReview>
{
    Task<IEnumerable<JobReview>> GetByJobApplicationAsync(Guid jobApplicationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobReview>> GetBySubjectAsync(Guid subjectId, ReputationRole role, CancellationToken cancellationToken = default);
    Task<bool> HasReviewedAsync(Guid jobApplicationId, Guid raterId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<JobReview> Items, int TotalCount)> GetPagedReviewsAdminAsync(
        int page,
        int pageSize,
        int? rating = null,
        string? search = null,
        CancellationToken cancellationToken = default);
}