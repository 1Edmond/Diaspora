using IdentityService.Application.Abstractions;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class UserPermissionRepository : IUserPermissionRepository
{
    private readonly IdentityDbContext _context;

    public UserPermissionRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<UserPermission?> GetUserPermissionAsync(
        Guid userId,
        string permissionKey,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserPermissions
            .FirstOrDefaultAsync(
                up => up.UserId == userId && up.PermissionKey == permissionKey,
                cancellationToken);
    }

    public async Task<List<UserPermission>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var permissions = await _context.UserPermissions
            .Where(up => up.UserId == userId)
            .ToListAsync(cancellationToken);
        var userRole = await _context.UserRoles.Where(up => up.UserId == userId).ToListAsync();

        var rolePermissions = await _context.RolePermissions
            .Where(rp => userRole.Any(ur => ur.RoleId == rp.RoleId)).ToListAsync();

        permissions.AddRange(
            rolePermissions.Select(rp => new UserPermission()
            {
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(25),
                GrantedAt = rp.GrantedAt,
                GrantedBy = rp.GrantedBy,
                PermissionKey = rp.PermissionKey,
                IsGranted = true
            })
            );

        return permissions;
    }

    public async Task GrantPermissionToUserAsync(
        Guid userId,
        string permissionKey,
        Guid? grantedBy = null,
        DateTime? expiresAt = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetUserPermissionAsync(userId, permissionKey, cancellationToken);

        if (existing != null)
        {
            existing.IsGranted = true;
            existing.GrantedAt = DateTime.UtcNow;
            existing.GrantedBy = grantedBy;
            existing.ExpiresAt = expiresAt;
        }
        else
        {
            await _context.UserPermissions.AddAsync(new UserPermission
            {
                UserId = userId,
                PermissionKey = permissionKey,
                IsGranted = true,
                GrantedBy = grantedBy,
                ExpiresAt = expiresAt,
                GrantedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DenyPermissionToUserAsync(
        Guid userId,
        string permissionKey,
        Guid? grantedBy = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetUserPermissionAsync(userId, permissionKey, cancellationToken);

        if (existing != null)
        {
            existing.IsGranted = false;
            existing.GrantedAt = DateTime.UtcNow;
            existing.GrantedBy = grantedBy;
        }
        else
        {
            await _context.UserPermissions.AddAsync(new UserPermission
            {
                UserId = userId,
                PermissionKey = permissionKey,
                IsGranted = false,
                GrantedBy = grantedBy,
                GrantedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveUserPermissionAsync(
        Guid userId,
        string permissionKey,
        CancellationToken cancellationToken = default)
    {
        await _context.UserPermissions
            .Where(up => up.UserId == userId && up.PermissionKey == permissionKey)
            .ExecuteDeleteAsync(cancellationToken);
    }
}