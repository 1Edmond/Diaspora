using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Interfaces;

public interface IJobApplicationRepository : IRepository<JobApplication>
{
    Task<IEnumerable<JobApplication>> GetByWorkerAsync(Guid workerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobApplication>> GetByJobPostingAsync(Guid jobPostingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobApplication>> GetByEmployerAsync(Guid employerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobApplication>> GetByStatusAsync(Guid jobPostingId, JobApplicationStatus status, CancellationToken cancellationToken = default);
    Task<int> CountCreatedSinceAsync(Guid workerId, DateTime since, CancellationToken cancellationToken = default);
}