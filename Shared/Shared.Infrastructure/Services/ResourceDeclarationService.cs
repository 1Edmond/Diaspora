using Shared.Application.Abstractions;
using Shared.Domain.Models;
using Shared.Infrastructure.Scanners;
using Shared.Messaging.Events.Authorization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Shared.Infrastructure.Services;

public class ResourceDeclarationService : IResourceDeclarationService
{

    //public void RegisterResource(
    //    string resourceName,
    //    string resourceKey,
    //    string description,
    //    params PermissionDeclaration[] permissions)
    //{
    //    _resources.Add(new ResourceDeclaration
    //    {
    //        ResourceName = resourceName,
    //        ResourceKey = resourceKey,
    //        Description = description,
    //        Permissions = permissions.ToList()
    //    });
    //}

    public ResourcesDeclarationIntegrationEvent BuildDeclarationEventFromAssembly(string serviceName, Assembly assembly)
    {
        var groups = PermissionScanner.ScanAssembly(assembly);

        var resources = groups.Select(g => new ResourceDeclaration
        {
            ResourceName = g.ResourceName,
            ResourceKey = g.ResourceKey,
            Description = $"{g.ResourceName} management",
            Permissions = g.Permissions.Select(p => new PermissionDeclaration
            {
                Permission = p.Action,
                Description = p.Description,
                RecommendedRoles = p.RecommendedRoles,
                //HttpMethod = p.HttpMethod,
                //PathPrefix = p.PathPrefix
            }).ToList()
        }).ToList();
        return new ResourcesDeclarationIntegrationEvent
        {
            ServiceName = serviceName,
            Resources = resources
        };
    }
}