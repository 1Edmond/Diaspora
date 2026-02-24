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

        var grantedPermissions = httpContext.Request.Headers["X-Granted-Permission"]
            .ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToArray();

        if (grantedPermissions.Length == 0)
        {
            _logger.LogWarning(
                "[PermissionFilter] ⛔ No X-Granted-Permission header found. Required: {Permission}",
                requiredPermission
            );

            context.Result = new UnauthorizedObjectResult(new
            {
                Error = "No permission header. Request must go through API Gateway."
            });
            return;
        }

        if (!grantedPermissions.Any(p => p.Equals(requiredPermission, StringComparison.OrdinalIgnoreCase)))
        {
            var userId = httpContext.Request.Headers["X-User-Id"].ToString();

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