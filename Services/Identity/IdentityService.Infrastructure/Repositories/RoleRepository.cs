using IdentityService.Application.Abstractions;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class RoleRepository(IdentityDbContext context) : IRoleRepository
{
    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await context.Roles.AddAsync(role, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        context.Roles.Update(role);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.Roles
            .Where(r => r.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Roles
            .Include(r => r.RolePermissions)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RolePermission>> GetRolePermissionsAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddPermissionToRoleAsync(
        Guid roleId,
        string permissionKey,
        Guid? grantedBy = null,
        CancellationToken cancellationToken = default)
    {
        var exists = await context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionKey == permissionKey, cancellationToken);

        if (!exists)
        {
            await context.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = roleId,
                PermissionKey = permissionKey,
                GrantedBy = grantedBy,
                GrantedAt = DateTime.UtcNow
            }, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemovePermissionFromRoleAsync(
        Guid roleId,
        string permissionKey,
        CancellationToken cancellationToken = default)
    {
        await context.RolePermissions
            .Where(rp => rp.RoleId == roleId && rp.PermissionKey == permissionKey)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<bool> RoleHasPermissionAsync(
        Guid roleId,
        string permissionKey,
        CancellationToken cancellationToken = default)
    {
        return await context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionKey == permissionKey, cancellationToken);
    }
}