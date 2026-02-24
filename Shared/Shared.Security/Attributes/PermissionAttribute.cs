using Microsoft.AspNetCore.Mvc;
using Shared.Security.Handlers;
using System;

namespace Shared.Security.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class PermissionAttribute : TypeFilterAttribute
{
    public string ResourceKey { get; }
    public string Action { get; }
    public string[] RecommendedRoles { get; }
    public string Description { get; }

    public PermissionAttribute(
        string resourceKey,
        string action,
        string description = "",
        params string[] recommendedRoles) : base(typeof(PermissionAuthorizationFilter))
    {
        ResourceKey = resourceKey;
        Action = action;
        Description = description;
        RecommendedRoles = recommendedRoles;

        Arguments = new object[] { this };
    }
}
