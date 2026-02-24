using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Domain.Entities;

public class RolePermission
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }

    public string PermissionKey { get; set; } = string.Empty;

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    public Guid? GrantedBy { get; set; }

    public Role Role { get; set; } = null!;
}