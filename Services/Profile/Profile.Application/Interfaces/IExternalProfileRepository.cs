using Profile.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Interfaces;


public interface IExternalProfileRepository : IRepository<ExternalProfile>
{
    Task<IEnumerable<ExternalProfile>> GetByInternalProfileIdAsync(Guid internalProfileId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExternalProfile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ExternalProfile?> GetByUserIdAndTypeAsync(Guid userId, Guid externalProfileTypeId, CancellationToken cancellationToken = default);
}