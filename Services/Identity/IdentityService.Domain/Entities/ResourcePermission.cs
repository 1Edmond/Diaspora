using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Domain.Entities;

public class ResourcePermission
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public string ResourceKey { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public string PermissionKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}