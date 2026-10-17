using IdentityService.Application.Abstractions;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using Shared.Security.Helpers;

namespace IdentityService.Infrastructure.Repositories;

public class UserRepository(
    IdentityDbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<User>> logger,
    IDistributedCache cache)
    : Repository<User>(
        context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IUserRepository
{
    private readonly IdentityDbContext _context = context;

    public async Task<User?> GetByRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(
                u => u.RefreshTokens.Any(t => t.Token == token),
                cancellationToken);

        if (user is not null)
        {
            await EncryptedValueObjectHelper.DecryptEntityAsync(
                user, EncryptionService, cancellationToken);
        }

        return user;
    }
}
