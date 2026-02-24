using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Shared.Domain.Attributes;
using Shared.Security.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Shared.Infrastructure.Scanners;

public class PermissionScanner
{
    public static List<ResourceDeclarationGroup> ScanAssembly(Assembly assembly)
    {
        var groups = new Dictionary<string, ResourceDeclarationGroup>();

        var controllers = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ControllerBase)) && !t.IsAbstract);

        foreach (var controller in controllers)
        {
            var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<PermissionAttribute>() != null);

            foreach (var method in methods)
            {
                var permAttr = method.GetCustomAttribute<PermissionAttribute>()!;
                var httpMethod = GetHttpMethod(method);
                var routeTemplate = GetRouteTemplate(controller, method);

                if (!groups.TryGetValue(permAttr.ResourceKey, out var group))
                {
                    group = new ResourceDeclarationGroup
                    {
                        ResourceKey = permAttr.ResourceKey,
                        ResourceName = GetResourceName(permAttr.ResourceKey),
                    };
                    groups[permAttr.ResourceKey] = group;
                }

                group.Permissions.Add(new PermissionDeclarationItem
                {
                    Action = permAttr.Action,
                    Description = permAttr.Description,
                    RecommendedRoles = permAttr.RecommendedRoles.ToList(),
                    HttpMethod = httpMethod,
                    PathPrefix = NormalizeRoute(routeTemplate)
                });
            }
        }

        return groups.Values.ToList();
    }

    private static string GetHttpMethod(MethodInfo method)
    {
        if (method.GetCustomAttribute<HttpGetAttribute>() != null) return "GET";
        if (method.GetCustomAttribute<HttpPostAttribute>() != null) return "POST";
        if (method.GetCustomAttribute<HttpPutAttribute>() != null) return "PUT";
        if (method.GetCustomAttribute<HttpPatchAttribute>() != null) return "PATCH";
        if (method.GetCustomAttribute<HttpDeleteAttribute>() != null) return "DELETE";
        return "GET";
    }

    private static string GetRouteTemplate(Type controller, MethodInfo method)
    {
        var controllerRoute = controller.GetCustomAttribute<RouteAttribute>()?.Template ?? "";

        var methodRoute = method.GetCustomAttributes()
            .OfType<HttpMethodAttribute>()
            .FirstOrDefault()?.Template ?? "";

        var fullRoute = string.IsNullOrEmpty(methodRoute)
            ? controllerRoute
            : $"{controllerRoute}/{methodRoute}";

        return fullRoute.TrimStart('/');
    }

    private static string NormalizeRoute(string route)
    {
        return route
            .Replace("{verificationId:guid}", "{id}")
            .Replace("{documentId:guid}", "{id}")
            .Replace("{id:guid}", "{id}")
            .TrimStart('/');
    }

    private static string GetResourceName(string resourceKey)
    {
        var parts = resourceKey.Split('.');
        return string.Join("", parts.Select(p =>
            char.ToUpper(p[0]) + p[1..]
        ));
    }
}


public class ResourceDeclarationGroup
{
    public string ResourceKey { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public List<PermissionDeclarationItem> Permissions { get; set; } = new();
}

public class PermissionDeclarationItem
{
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RecommendedRoles { get; set; } = new();
    public string HttpMethod { get; set; } = string.Empty;
    public string PathPrefix { get; set; } = string.Empty;
}