using Freelance.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Interfaces;

public interface IJobPostingRepository : IRepository<JobPosting>
{
    Task<IEnumerable<JobPosting>> GetByEmployerAsync(Guid employerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobPosting>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<int> CountCreatedSinceAsync(Guid employerId, DateTime since, CancellationToken cancellationToken = default);
}