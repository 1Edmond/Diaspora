using KYC.Application.Abstractions;
using KYC.Domain.Entities;
using KYC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KYC.Infrastructure.Repositories;

public class ProfileDirectoryRepository(KYCDbContext context) : IProfileDirectoryRepository
{
    public Task<ProfileDirectoryEntry?> GetByIdAsync(Guid profileId, CancellationToken cancellationToken = default)
        => context.ProfileDirectoryEntries
            .FirstOrDefaultAsync(e => e.Id == profileId, cancellationToken);

    public Task<ProfileDirectoryEntry?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => context.ProfileDirectoryEntries
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

    public async Task AddAsync(ProfileDirectoryEntry entry, CancellationToken cancellationToken = default)
        => await context.ProfileDirectoryEntries.AddAsync(entry, cancellationToken);

    public Task UpdateAsync(ProfileDirectoryEntry entry, CancellationToken cancellationToken = default)
    {
        context.ProfileDirectoryEntries.Update(entry);
        return Task.CompletedTask;
    }
}
