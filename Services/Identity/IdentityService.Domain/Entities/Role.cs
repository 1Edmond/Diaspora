using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Domain.Entities;

public class Role : AggregateRoot
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public void AddPermission(string permissionKey, Guid? grantedBy = null)
    {
        if (RolePermissions.Any(rp => rp.PermissionKey.Equals(permissionKey, StringComparison.OrdinalIgnoreCase)))
            return;

        RolePermissions.Add(new RolePermission
        {
            RoleId = Id,
            PermissionKey = permissionKey,
            GrantedBy = grantedBy,
            GrantedAt = DateTime.UtcNow
        });
    }
}
