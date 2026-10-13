using Freelance.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Interfaces;

public interface IWorkerVerificationStatusRepository : IRepository<WorkerVerificationStatus>
{
    Task<WorkerVerificationStatus?> GetByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default);
}