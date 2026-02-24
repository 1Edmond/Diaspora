using IdentityService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Abstractions;

public interface IRoleRepository
{
    Task AddAsync(Role role, CancellationToken cancellationToken = default);
    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<RolePermission>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task AddPermissionToRoleAsync(Guid roleId, string permissionKey, Guid? grantedBy = null, CancellationToken cancellationToken = default);
    Task RemovePermissionFromRoleAsync(Guid roleId, string permissionKey, CancellationToken cancellationToken = default);
    Task<bool> RoleHasPermissionAsync(Guid roleId, string permissionKey, CancellationToken cancellationToken = default);
}
