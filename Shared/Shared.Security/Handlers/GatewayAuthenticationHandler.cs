using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace Shared.Security.Handlers;

public class GatewayAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public GatewayAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        IConfiguration configuration,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var path = Request.Path;
        if (path.StartsWithSegments("/health") || path.StartsWithSegments("/alive"))
            return Task.FromResult(AuthenticateResult.NoResult());

        var gatewaySecret = Request.Headers["X-Gateway-Secret"].ToString();
        var configuredSecret = _configuration["Gateway:Secret"] ?? "";

        // Comparaison en temps constant : évite les attaques par timing sur le secret partagé
        if (!FixedTimeEquals(configuredSecret, gatewaySecret))
        {
            Logger.LogWarning("[GatewayAuth] ⛔ Secret Gateway invalide ou absent sur {Path}", path);
            return Task.FromResult(AuthenticateResult.Fail("Invalid gateway secret"));
        }

        // Route explicitement publique (register, login...) : pas d'identité attendue.
        // On laisse passer en anonyme, c'est au endpoint d'être [AllowAnonymous].
        var requireAuth = Request.Headers["X-Gateway-Require-Auth"].ToString();
        if (requireAuth.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userIdRaw = Request.Headers["X-User-Id"].ToString();
        var roles = Request.Headers["X-User-Roles"].ToString();

        // Route censée être authentifiée mais sans userId exploitable : on NE crée PAS
        // de principal fantôme. NoResult => les endpoints [Authorize] rejetteront proprement.
        if (string.IsNullOrEmpty(userIdRaw) || !Guid.TryParse(userIdRaw, out var userId))
        {
            Logger.LogWarning(
                "[GatewayAuth] ⛔ X-User-Id manquant ou invalide sur route protégée: {Path}", path);
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        foreach (var role in roles.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
        }

        var identity = new ClaimsIdentity(claims, "Gateway");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Gateway");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var bytesA = Encoding.UTF8.GetBytes(a);
        var bytesB = Encoding.UTF8.GetBytes(b);
        if (bytesA.Length != bytesB.Length) return false;
        return CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
    }
}