using Freelance.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Interfaces;

public interface IJobTemplateRepository : IRepository<JobTemplate>
{
    Task<IEnumerable<JobTemplate>> GetByEmployerAsync(Guid employerId, CancellationToken cancellationToken = default);
}