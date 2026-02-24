using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.Models;

public class ResourceDeclaration
{
    public string ResourceName { get; set; } = string.Empty;
    public string ResourceKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<PermissionDeclaration> Permissions { get; set; } = new();
}
