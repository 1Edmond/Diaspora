using IdentityService.Application.Models;
using IdentityService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Abstractions;

public interface IPermissionService
{
    Task<bool> UserHasPermissionAsync(Guid userId,
    string path,
    string method,
    CancellationToken cancellationToken = default);

    Task<UserAuthorisationResponse?> GetUserPermissionsAsync(
    Guid userId,
    CancellationToken cancellationToken = default);

    Task<List<ResourcePermission>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
    Task AssignPermissionToRoleAsync(Guid roleId, string permissionKey, Guid? grantedBy = null, CancellationToken cancellationToken = default);
    Task AssignPermissionsToRoleAsync(Guid roleId, List<string> permissionKey, Guid? grantedBy = null, CancellationToken cancellationToken = default);
    Task RemovePermissionFromRoleAsync(Guid roleId, string permissionKey, CancellationToken cancellationToken = default);
}
