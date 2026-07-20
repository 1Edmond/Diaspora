using Profile.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Interfaces;

public interface IInternalProfileRepository : IRepository<InternalProfile>
{
    Task<InternalProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InternalProfile>> GetPendingVerificationAsync(CancellationToken cancellationToken = default);
}
