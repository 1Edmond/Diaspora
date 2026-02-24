using IdentityService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Abstractions;

public interface IResourcePermissionRepository
{
    Task<ResourcePermission?> GetByPermissionKeyAsync(string permissionKey, CancellationToken cancellationToken = default);
    Task<List<ResourcePermission>> GetByServiceNameAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<List<ResourcePermission>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task UpsertAsync(ResourcePermission resourcePermission, CancellationToken cancellationToken = default);
    Task UpsertBatchAsync(List<ResourcePermission> resourcePermissions, CancellationToken cancellationToken = default);
}