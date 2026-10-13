using Freelance.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Interfaces;

public interface IJobCheckInRepository : IRepository<JobCheckIn>
{
    Task<IEnumerable<JobCheckIn>> GetByJobApplicationAsync(Guid jobApplicationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobCheckIn>> GetByWorkerAsync(Guid workerId, CancellationToken cancellationToken = default);
}