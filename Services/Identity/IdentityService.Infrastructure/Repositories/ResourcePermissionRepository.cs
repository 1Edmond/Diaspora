using IdentityService.Application.Abstractions;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class ResourcePermissionRepository : IResourcePermissionRepository
{
    private readonly IdentityDbContext _context;

    public ResourcePermissionRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<ResourcePermission?> GetByPermissionKeyAsync(
        string permissionKey,
        CancellationToken cancellationToken = default)
    {
        return await _context.ResourcePermissions
            .AsNoTracking()
            .FirstOrDefaultAsync(rp => rp.PermissionKey == permissionKey, cancellationToken);
    }

    public async Task<List<ResourcePermission>> GetByServiceNameAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        return await _context.ResourcePermissions
            .AsNoTracking()
            .Where(rp => rp.ServiceName == serviceName && rp.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ResourcePermission>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ResourcePermissions
            .AsNoTracking()
            .Where(rp => rp.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(
        ResourcePermission resourcePermission,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.ResourcePermissions
            .FirstOrDefaultAsync(rp => rp.PermissionKey == resourcePermission.PermissionKey, cancellationToken);

        if (existing != null)
        {
            existing.ServiceName = resourcePermission.ServiceName;
            existing.ResourceName = resourcePermission.ResourceName;
            existing.ResourceKey = resourcePermission.ResourceKey;
            existing.Permission = resourcePermission.Permission;
            existing.Description = resourcePermission.Description;
            existing.RegisteredAt = DateTime.UtcNow;
            existing.IsActive = true;
        }
        else
        {
            await _context.ResourcePermissions.AddAsync(resourcePermission, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertBatchAsync(
        List<ResourcePermission> resourcePermissions,
        CancellationToken cancellationToken = default)
    {
        foreach (var permission in resourcePermissions)
        {
            await UpsertAsync(permission, cancellationToken);
        }
    }
}