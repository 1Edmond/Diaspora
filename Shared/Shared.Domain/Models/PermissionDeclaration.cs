using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.Models;
public class PermissionDeclaration
{
    public string Permission { get; set; } = string.Empty;      
    public string Description { get; set; } = string.Empty;
    public List<string> RecommendedRoles { get; set; } = [];

    public static PermissionDeclaration Create(string permission, string description = "", params string[] recommendedRoles)
        => new() { Permission = permission, Description = description, RecommendedRoles = [..recommendedRoles] };
}
