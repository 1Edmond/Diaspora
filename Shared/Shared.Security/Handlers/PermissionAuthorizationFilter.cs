using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Shared.Domain.Attributes;
using Shared.Security.Attributes;

namespace Shared.Security.Handlers;

public class PermissionAuthorizationFilter : IAuthorizationFilter
{
    private readonly PermissionAttribute _permission;
    private readonly ILogger<PermissionAuthorizationFilter> _logger;

    public PermissionAuthorizationFilter(
        PermissionAttribute permission,
        ILogger<PermissionAuthorizationFilter> logger)
    {
        _permission = permission;
        _logger = logger;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var requiredPermission = $"{_permission.ResourceKey}.{_permission.Action}";

        // 1. Check if user roles from gateway match recommended roles (e.g. User or Admin)
        var userRoles = httpContext.Request.Headers["X-User-Roles"]
            .ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(r => r.Trim())
            .ToArray();

        var userId = httpContext.Request.Headers["X-User-Id"].ToString();

        if (_permission.RecommendedRoles != null && _permission.RecommendedRoles.Length > 0)
        {
            var hasMatchingRole = userRoles.Any(r =>
                _permission.RecommendedRoles.Contains(r, StringComparer.OrdinalIgnoreCase));

            // Authenticated user with explicitly matched role
            if (hasMatchingRole)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug(
                        "[PermissionFilter] ✅ Role-based permission granted for {Permission} (User={UserId}, Roles=[{Roles}])",
                        requiredPermission, userId, string.Join(", ", userRoles)
                    );
                return;
            }
        }

        var grantedPermissions = httpContext.Request.Headers["X-Granted-Permission"]
            .ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToArray();

        if (grantedPermissions.Length == 0)
        {
            _logger.LogWarning(
                "[PermissionFilter] ⛔ No granted permissions found for user {UserId}. Required: {Permission}",
                userId,
                requiredPermission
            );

            context.Result = new ForbidResult();
            return;
        }

        if (!grantedPermissions.Any(p => p.Equals(requiredPermission, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning(
                "[PermissionFilter] ⛔ Permission mismatch: User={UserId}, Required={Required}, Granted=[{Granted}]",
                userId,
                requiredPermission,
                string.Join(", ", grantedPermissions)
            );

            context.Result = new ForbidResult();
            return;
        }

        if(_logger.IsEnabled(LogLevel.Debug))
        _logger.LogDebug(
            "[PermissionFilter] ✅ Permission granted: {Permission}",
            requiredPermission
        );
    }
}