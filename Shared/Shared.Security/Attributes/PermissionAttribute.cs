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
        if (resourceKey.Contains('.') && string.IsNullOrWhiteSpace(description) && (action.Contains(' ') || action.Length > 20))
        {
            var parts = resourceKey.Split('.', 2);
            ResourceKey = parts[0];
            Action = parts[1];
            Description = action;
            RecommendedRoles = recommendedRoles ?? Array.Empty<string>();
        }
        else
        {
            ResourceKey = resourceKey;
            Action = action;
            Description = description;
            RecommendedRoles = recommendedRoles ?? Array.Empty<string>();
        }

        Arguments = new object[] { this };
    }
}
