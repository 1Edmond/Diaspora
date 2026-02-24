using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public string? AssignedBy { get; set; }
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}