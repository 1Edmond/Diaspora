using ServcicesGateway.Interfaces;
using ServcicesGateway.Models;
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
        "/api/auth/resend-verification",
        "/api/auth/ping",
        "/api/profile-types/public",
        "/api/document-types/public",
        "/api/config/mobile",
        "/api/marketplace/payment-policies",
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
        context.Request.Headers.Remove("X-User-Id");
        context.Request.Headers.Remove("X-User-Email");
        context.Request.Headers.Remove("X-User-Roles");
        context.Request.Headers.Remove("X-Granted-Permission");
        context.Request.Headers.Remove("X-Gateway-Require-Auth");
        context.Request.Headers.Remove("X-Gateway-Secret");

        var path = context.Request.Path.Value ?? "";

        // 1. Si l'utilisateur est authentifié avec un jeton valide, propager son identité et ses permissions
        if (context.User.Identity is { IsAuthenticated: true })
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? context.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                var userPermissions = await authorizationService.GetPermissionAsync(userId);
                if (userPermissions != null)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("[Gateway] ✅ Granted: User={UserId}, Path={Path}", userId, path);

                    AddUserHeaders(configuration, context, userId, userPermissions);
                    await _next(context);
                    return;
                }
            }
        }

        // 2. Si non authentifié mais route explicitement publique
        if (IsPublicRoute(path))
        {
            await HandlePublicRouteAsync(context, configuration);
            return;
        }

        // 3. Rejet 401 si non authentifié sur route protégée
        await WriteError(context, StatusCodes.Status401Unauthorized, "Unauthorized");
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

    private static void AddUserHeaders(IConfiguration configuration, HttpContext context, Guid userId, AuthorizationCheckResponse userPermissions)
    {
        context.Request.Headers["X-Gateway-Secret"] = configuration["Gateway:Secret"];
        context.Request.Headers["X-User-Id"] = userId.ToString();

        var roles = (userPermissions.Roles ?? Array.Empty<string>())
            .Concat(context.User.FindAll(ClaimTypes.Role).Select(c => c.Value))
            .Concat(context.User.FindAll("role").Select(c => c.Value))
            .Concat(context.User.FindAll("roles").Select(c => c.Value))
            .Distinct();
        context.Request.Headers["X-User-Roles"] = string.Join(",", roles);

        context.Request.Headers["X-Granted-Permission"] = string.Join(",", userPermissions.Permissions ?? Array.Empty<string>());
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var bytesA = Encoding.UTF8.GetBytes(a);
        var bytesB = Encoding.UTF8.GetBytes(b);
        if (bytesA.Length != bytesB.Length) return false;
        return CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
    }
}