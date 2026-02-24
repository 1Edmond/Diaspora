using Shared.Domain.Models;
using Shared.Messaging.Events.Authorization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Shared.Application.Abstractions;

public interface IResourceDeclarationService
{
    //void RegisterResource(
    //    string resourceName,
    //    string resourceKey,
    //    string description,
    //    params PermissionDeclaration[] permissions);

    //ResourcesDeclarationIntegrationEvent BuildDeclarationEvent(string serviceName);

    ResourcesDeclarationIntegrationEvent BuildDeclarationEventFromAssembly(
        string serviceName,
        Assembly assembly);
}