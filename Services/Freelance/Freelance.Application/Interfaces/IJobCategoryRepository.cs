using Freelance.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Interfaces;

public interface IJobCategoryRepository : IRepository<JobCategory>
{
    Task<IEnumerable<JobCategory>> GetActiveAsync(CancellationToken cancellationToken = default);
}