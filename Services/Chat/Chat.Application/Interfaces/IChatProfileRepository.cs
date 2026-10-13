using Chat.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Interfaces;

public interface IChatProfileRepository : IRepository<ChatProfile>
{
    Task<ChatProfile?> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default);

    Task<ChatProfile?> GetByProfileIdAsync(
        Guid externalProfileId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<ChatProfile>> GetByIdsAsync(
        IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}