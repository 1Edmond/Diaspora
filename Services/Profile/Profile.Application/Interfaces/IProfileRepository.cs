using Profile.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Interfaces;

public interface IProfileRepository : IRepository<Domain.Entities.Profile>
{
    Task<List<Domain.Entities.Profile>?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.Profile>> GetPendingVerificationAsync(CancellationToken cancellationToken = default);
}
