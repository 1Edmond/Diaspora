using Freelance.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Interfaces;

public interface IWorkerJobPreferenceRepository : IRepository<WorkerJobPreference>
{
    Task<WorkerJobPreference?> GetByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default);
}