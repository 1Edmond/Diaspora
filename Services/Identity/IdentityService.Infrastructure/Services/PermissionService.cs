using IdentityService.Application.Abstractions;
using IdentityService.Application.Models;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;

namespace IdentityService.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly IResourcePermissionRepository _resourcePermissionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserPermissionRepository _userPermissionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        IResourcePermissionRepository resourcePermissionRepository,
        IRoleRepository roleRepository,
        IUserPermissionRepository userPermissionRepository,
        IRepository<User> userRepository,
        ILogger<PermissionService> logger)
    {
        _resourcePermissionRepository = resourcePermissionRepository;
        _roleRepository = roleRepository;
        _userPermissionRepository = userPermissionRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<bool> UserHasPermissionAsync(
        Guid userId,
        string path,
        string method,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return false;

        var userRoles = user.UserRoles.Select(ur => ur.Role).ToList();

        var permissionKey = await ResolvePermissionKeyAsync(path, method, cancellationToken);

        if (permissionKey == null)
        {
            _logger.LogWarning(
                "[PermissionService] No permission found for {Method} {Path}",
                method, path);
            return false;
        }

        var hasRolePermission = userRoles.Any(role =>
            role.RolePermissions.Any(rp =>
                rp.PermissionKey.Equals(permissionKey, StringComparison.OrdinalIgnoreCase)));

        if (hasRolePermission)
            return true;

        var userPermission = await _userPermissionRepository.GetUserPermissionAsync(
            userId, permissionKey, cancellationToken);

        if (userPermission != null && userPermission.IsValid())
            return userPermission.IsGranted;

        _logger.LogDebug(
            "[PermissionService] User {UserId} does not have permission {PermissionKey}",
            userId, permissionKey);

        return false;
    }

    public async Task<UserAuthorisationResponse?> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return null;

        var userRoles = user.UserRoles.Select(ur => ur.Role).ToList();

        var allPermissions = await _resourcePermissionRepository.GetAllActiveAsync(cancellationToken);

        var response = new UserAuthorisationResponse
        {
            Roles = [.. userRoles.Select(r => r.Name)],
            Permissions = [.. allPermissions
                .Where(p => userRoles.Any(role =>
                    role.RolePermissions.Any(rp =>
                        rp.PermissionKey.Equals(p.PermissionKey, StringComparison.OrdinalIgnoreCase))))
                .Select(p => p.PermissionKey)]
        };

        return response;
    }

    public async Task<List<ResourcePermission>> GetAllPermissionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _resourcePermissionRepository.GetAllActiveAsync(cancellationToken);
    }

    public async Task AssignPermissionToRoleAsync(
        Guid roleId,
        string permissionKey,
        Guid? grantedBy = null,
        CancellationToken cancellationToken = default)
    {
        await _roleRepository.AddPermissionToRoleAsync(
            roleId,
            permissionKey,
            grantedBy,
            cancellationToken);

        _logger.LogInformation(
            "[PermissionService] Permission assigned to role: Role={RoleId}, Permission={Permission}",
            roleId, permissionKey);
    }

    public async Task AssignPermissionsToRoleAsync(
        Guid roleId,
        List<string> permissionKeys,
        Guid? grantedBy = null,
        CancellationToken cancellationToken = default)
    {
        var tasks = permissionKeys.Select(key =>
            AssignPermissionToRoleAsync(roleId, key, grantedBy, cancellationToken));

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "[PermissionService] Permissions assigned to role: Role={RoleId}, Count={Count}",
            roleId, permissionKeys.Count);
    }

    public async Task RemovePermissionFromRoleAsync(
        Guid roleId,
        string permissionKey,
        CancellationToken cancellationToken = default)
    {
        await _roleRepository.RemovePermissionFromRoleAsync(
            roleId,
            permissionKey,
            cancellationToken);

        _logger.LogInformation(
            "[PermissionService] Permission removed from role: Role={RoleId}, Permission={Permission}",
            roleId, permissionKey);
    }

    private async Task<string?> ResolvePermissionKeyAsync(
        string path,
        string method,
        CancellationToken cancellationToken)
    {
        var allPermissions = await _resourcePermissionRepository.GetAllActiveAsync(cancellationToken);

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2) return null;

        var serviceSegment = segments[1];
        string resourceSegment;
        string action;

        // Pattern 1: /api/profiles/{guid}/reactivate or /api/profiles/{guid}
        if (segments.Length >= 3 && Guid.TryParse(segments[2], out _))
        {
            resourceSegment = segments[1];
            action = segments.Length >= 4 ? segments[3].ToLower() : MapMethodToAction(method);
        }
        // Pattern 2: /api/service/resource/{guid}/action
        else if (segments.Length >= 4 && Guid.TryParse(segments[3], out _))
        {
            resourceSegment = segments[2];
            action = segments.Length >= 5 ? segments[4].ToLower() : MapMethodToAction(method);
        }
        // Pattern 3: /api/service/resource
        else if (segments.Length >= 3)
        {
            resourceSegment = segments[2];
            action = segments.Length >= 4 ? segments[3].ToLower() : MapMethodToAction(method);
        }
        else
        {
            resourceSegment = segments[1];
            action = MapMethodToAction(method);
        }

        // Aliases
        if (action == "reactivate") action = "activate";
        if (action == "status") action = "update";

        var matchingPermissions = allPermissions.Where(p =>
            p.ServiceName.Equals(serviceSegment, StringComparison.OrdinalIgnoreCase) ||
            p.ResourceKey.Contains(serviceSegment, StringComparison.OrdinalIgnoreCase) ||
            p.ServiceName.Equals(resourceSegment, StringComparison.OrdinalIgnoreCase) ||
            p.ResourceKey.Contains(resourceSegment, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        if (matchingPermissions.Count == 0) return null;

        var permission = matchingPermissions.FirstOrDefault(p =>
            p.Permission.Equals(action, StringComparison.OrdinalIgnoreCase) &&
            (p.ResourceKey.Contains(resourceSegment.TrimEnd('s'), StringComparison.OrdinalIgnoreCase) ||
             resourceSegment.Contains(p.ResourceKey, StringComparison.OrdinalIgnoreCase) ||
             p.ResourceKey.Contains(serviceSegment.TrimEnd('s'), StringComparison.OrdinalIgnoreCase)));

        permission ??= matchingPermissions.FirstOrDefault(p =>
            p.Permission.Equals(action, StringComparison.OrdinalIgnoreCase));

        _logger.LogDebug(
            "[PermissionService] Resolved: {Method} {Path} → {Permission}",
            method, path, permission?.PermissionKey ?? "null");

        return permission?.PermissionKey;
    }

    private static string MapMethodToAction(string method) => method.ToUpper() switch
    {
        "GET" => "read",
        "POST" => "create",
        "PUT" => "update",
        "PATCH" => "update",
        "DELETE" => "delete",
        _ => "read"
    };
}