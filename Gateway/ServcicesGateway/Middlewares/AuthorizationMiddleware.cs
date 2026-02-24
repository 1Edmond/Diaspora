using ServcicesGateway.Interfaces;
using System.Security;
using System.Security.Claims;

namespace ServcicesGateway.Middlewares;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationMiddleware> _logger;

    private static readonly HashSet<string> PublicPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/login",
        "/api/auth/register",
        "/api/auth/refresh",
        "/api/auth/verify-email",
        "/api/auth/ping",
        "/api/authorization/check",
        "/health",
        "/alive"
    };

    public AuthorizationMiddleware(
        RequestDelegate next,
        ILogger<AuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IConfiguration configuration,
        IAuthorizationService authorizationService)
    {
        var path = context.Request.Path.Value ?? "";
        if (IsPublicRoute(path))
        {
            _logger.LogDebug("[Gateway] 🔓 Public route: {Path}", path);
            await _next(context);
            return;
        }

        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { Error = "Unauthorized" });
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? context.User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { Error = "Invalid token" });
            return;
        }

        if(_logger.IsEnabled(LogLevel.Debug))
        _logger.LogDebug(
            "[Gateway] 🔍 Checking: User={UserId}, Path={Path}",
            userId, path
        );

        var userPermissions = await authorizationService.GetPermissionAsync(
            userId);

        if (userPermissions is null)
        {
            _logger.LogWarning(
                "[Gateway] ⛔ Forbidden: User={UserId}, Path={Path}",
                userId, path
            );

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "Forbidden",
                Message = "You do not have permission to access this resource"
            });
            return;
        }

        if(_logger.IsEnabled(LogLevel.Information))
        _logger.LogInformation(
            "[Gateway] ✅ Granted: User={UserId}, Path={Path}",
            userId, path
        );

        AddUserHeaders(configuration ,context, userId, userPermissions.Permissions);
        await _next(context);
    }

    private static bool IsPublicRoute(string path)
        => PublicPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

    private static void AddUserHeaders(IConfiguration configuration, HttpContext context, Guid userId, string[] permission)
    {
        context.Request.Headers["X-Gateway-Secret"] = configuration["Gateway:Secret"];
        context.Request.Headers["X-User-Id"] = userId.ToString();

        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        context.Request.Headers["X-User-Roles"] = string.Join(",", roles);

        context.Request.Headers["X-Granted-Permission"] = string.Join(",", permission);
    }
}