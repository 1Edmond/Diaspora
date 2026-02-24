using IdentityService.Application.Abstractions;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Serilog;
using Shared.Domain.Constants;
using Shared.Messaging.Events.Authorization;
using System.Xml.Linq;
using Wolverine.Transports;

namespace IdentityService.Handlers;

public class ResourcesDeclarationEventHandler
{
    private readonly IResourcePermissionRepository _resourcePermissionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<ResourcesDeclarationEventHandler> _logger;

    private static readonly Dictionary<string, Guid> RoleIds = new()
    {
        { DefaultRole.Admin, DefaultRole.GetRoleId(DefaultRole.Admin) },
        { DefaultRole.User, DefaultRole.GetRoleId(DefaultRole.User) },
        { DefaultRole.Moderator, DefaultRole.GetRoleId(DefaultRole.Moderator) }
    };

    public ResourcesDeclarationEventHandler(
        IResourcePermissionRepository resourcePermissionRepository,
        IRoleRepository roleRepository,
        ILogger<ResourcesDeclarationEventHandler> logger)
    {
        _resourcePermissionRepository = resourcePermissionRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task Handle(ResourcesDeclarationIntegrationEvent message)
    {
        if(_logger.IsEnabled(LogLevel.Information))
        _logger.LogInformation(
            "📦 Received resource declarations from {ServiceName}: {Count} resources",
            message.ServiceName,
            message.Resources.Count
        );

        try
        {
            var resourcePermissions = new List<ResourcePermission>();

            foreach (var resource in message.Resources)
            {
                foreach (var permission in resource.Permissions)
                {
                    var permissionKey = $"{resource.ResourceKey}.{permission.Permission}";

                    var resourcePermission = new ResourcePermission
                    {
                        ServiceName = message.ServiceName,
                        ResourceName = resource.ResourceName,
                        ResourceKey = resource.ResourceKey,
                        Permission = permission.Permission,
                        PermissionKey = permissionKey,
                        Description = permission.Description,
                        RegisteredAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    resourcePermissions.Add(resourcePermission);

                    if(_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation(
                        "✅ Registered permission: {PermissionKey}",
                        permissionKey
                    );

                    await AssignToRecommendedRoles(permissionKey, permission.RecommendedRoles);
                }
            }

            await _resourcePermissionRepository.UpsertBatchAsync(resourcePermissions);

            if(_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation(
                "🎉 Successfully registered {Count} permissions from {ServiceName}",
                resourcePermissions.Count,
                message.ServiceName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Error processing resource declarations from {ServiceName}",
                message.ServiceName
            );
            throw;
        }
    }

    private async Task AssignToRecommendedRoles(string permissionKey, List<string> recommendedRoles)
    {
        if (recommendedRoles.Count == 0)
        {
            if(_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug(
                "No recommended roles for permission: {Permission}",
                permissionKey
            );
            return;
        }

        foreach (var roleName in recommendedRoles)
        {
            if (!RoleIds.TryGetValue(roleName, out var roleId))
            {
                _logger.LogWarning(
                    "⚠️ Unknown role: {RoleName} for permission {Permission}",
                    roleName,
                    permissionKey
                );
                continue;
            }

            try
            {
                await _roleRepository.AddPermissionToRoleAsync(
                    roleId,
                    permissionKey,
                    grantedBy: null
                );

                if(_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation(
                    "✅ Assigned {Permission} to role {Role}",
                    permissionKey,
                    roleName
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to assign {Permission} to role {Role}",
                    permissionKey,
                    roleName
                );
            }
        }
    }
}