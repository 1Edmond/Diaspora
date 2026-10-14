using KYC.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KYC.Application.Abstractions;

public interface IProfileDirectoryRepository
{
    Task<ProfileDirectoryEntry?> GetByIdAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<ProfileDirectoryEntry?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(ProfileDirectoryEntry entry, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProfileDirectoryEntry entry, CancellationToken cancellationToken = default);
}
