using ServcicesGateway.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ServcicesGateway.Middlewares;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationMiddleware> _logger;

    private static readonly HashSet<string> PublicPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/login",
        "/api/auth/biometric/challenge",
        "/api/auth/biometric/login",
        "/api/auth/register",
        "/api/auth/refresh",
        "/api/auth/refresh-token",
        "/api/auth/verify-email",
        "/api/auth/ping",
        "/api/authorization/check",
        "/api/profile-types/public",
        "/api/document-types",
        "/health",
        "/alive"
    };

    public AuthorizationMiddleware(RequestDelegate next, ILogger<AuthorizationMiddleware> logger)
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
            await HandlePublicRouteAsync(context, configuration);
            return;
        }

        if (context.User.Identity is not { IsAuthenticated: true })
        {
            await WriteError(context, StatusCodes.Status401Unauthorized, "Unauthorized");
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? context.User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            await WriteError(context, StatusCodes.Status401Unauthorized, "Invalid token");
            return;
        }

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("[Gateway] 🔍 Checking: User={UserId}, Path={Path}", userId, path);

        var userPermissions = await authorizationService.GetPermissionAsync(userId);

        if (userPermissions is null)
        {
            _logger.LogWarning("[Gateway] ⛔ Forbidden: User={UserId}, Path={Path}", userId, path);
            await WriteError(context, StatusCodes.Status403Forbidden,
                "You do not have permission to access this resource");
            return;
        }

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("[Gateway] ✅ Granted: User={UserId}, Path={Path}", userId, path);

        AddUserHeaders(configuration, context, userId, userPermissions.Permissions);
        await _next(context);
    }

    private async Task HandlePublicRouteAsync(HttpContext context, IConfiguration configuration)
    {
        var key = context.Request.Headers["x-client-key"].ToString();
        var configuredKey = configuration["ClientKey"];

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(configuredKey)
            || !FixedTimeEquals(configuredKey, key))
        {
            await WriteError(context, StatusCodes.Status401Unauthorized, "Unauthorized");
            return;
        }

        _logger.LogDebug("[Gateway] 🔓 Public route: {Path}", context.Request.Path);
        context.Request.Headers["X-Gateway-Require-Auth"] = "false";
        context.Request.Headers["X-Gateway-Secret"] = configuration["Gateway:Secret"];
        await _next(context);
    }

    private static async Task WriteError(HttpContext context, int statusCode, string error)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { Error = error });
    }

    private static bool IsPublicRoute(string path)
        => PublicPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

    private static void AddUserHeaders(IConfiguration configuration, HttpContext context, Guid userId, string[] permissions)
    {
        context.Request.Headers["X-Gateway-Secret"] = configuration["Gateway:Secret"];
        context.Request.Headers["X-User-Id"] = userId.ToString();

        var roles = context.User.FindAll(ClaimTypes.Role)
            .Concat(context.User.FindAll("role"))
            .Concat(context.User.FindAll("roles"))
            .Select(c => c.Value)
            .Distinct();
        context.Request.Headers["X-User-Roles"] = string.Join(",", roles);

        context.Request.Headers["X-Granted-Permission"] = string.Join(",", permissions);
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var bytesA = Encoding.UTF8.GetBytes(a);
        var bytesB = Encoding.UTF8.GetBytes(b);
        if (bytesA.Length != bytesB.Length) return false;
        return CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
    }
}