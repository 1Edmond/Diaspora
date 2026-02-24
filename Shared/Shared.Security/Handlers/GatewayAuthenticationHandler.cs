using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
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

        var gatewaysSecret = Request.Headers["X-Gateway-Secret"].ToString();
        if (_configuration["Gateway:Secret"] != gatewaysSecret)
        {
            return Task.FromResult(AuthenticateResult.Fail("Unknown request"));
        }

        var userId = Request.Headers["X-User-Id"].ToString();
        var roles = Request.Headers["X-User-Roles"].ToString();

        
        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromResult(AuthenticateResult.Fail("No gateway headers found"));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("sub", userId)
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
}