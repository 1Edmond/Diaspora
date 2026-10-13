using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Interfaces;

public interface IFreelanceReputationRepository : IRepository<FreelanceReputation>
{
    Task<FreelanceReputation?> GetBySubjectAsync(Guid subjectId, ReputationRole role, CancellationToken cancellationToken = default);
}