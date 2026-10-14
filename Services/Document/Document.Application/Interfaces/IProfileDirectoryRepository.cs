using Document.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Document.Application.Interfaces;

public interface IProfileDirectoryRepository : IRepository<ProfileDirectoryEntry>
{
    Task<ProfileDirectoryEntry?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, ProfileDirectoryEntry>> GetByUserIdsAsync(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}
