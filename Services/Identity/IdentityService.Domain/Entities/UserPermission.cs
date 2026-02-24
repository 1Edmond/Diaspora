using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Domain.Entities;

public class UserPermission 
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string PermissionKey { get; set; } = string.Empty;

    public DateTime GrantedAt { get; set; }

    public Guid? GrantedBy { get; set; }

    public bool IsGranted { get; set; }

    public DateTime? ExpiresAt { get; set; }
    public bool IsValid() => !ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow;

    public UserPermission()
    {

    }




}
