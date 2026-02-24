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
}
