using IdentityService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Abstractions;

public interface IUserPermissionRepository
{
    Task<UserPermission?> GetUserPermissionAsync(Guid userId, string permissionKey, CancellationToken cancellationToken = default);
    Task<List<UserPermission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task GrantPermissionToUserAsync(Guid userId, string permissionKey, Guid? grantedBy = null, DateTime? expiresAt = null, CancellationToken cancellationToken = default);
    Task DenyPermissionToUserAsync(Guid userId, string permissionKey, Guid? grantedBy = null, CancellationToken cancellationToken = default);
    Task RemoveUserPermissionAsync(Guid userId, string permissionKey, CancellationToken cancellationToken = default);
}
