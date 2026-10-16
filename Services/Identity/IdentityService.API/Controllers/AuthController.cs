using IdentityService.Application.Commands;
using IdentityService.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Domain.Attributes;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class AuthController(IMessageBus _messageBus, ILogger<AuthController> logger, ICurrentUserService _currentUserService) : ControllerBase
{
   

    [AllowAnonymous]
    [HttpGet("ping")]
    public IActionResult HealthCheck()
    {
        logger.LogInformation("Ping checked");
        return Ok(new { status = "Identity Service is running" });
    }

 
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<Guid>>(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok();
    }


    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        command = command with { IpAddress = GetIpAddress() };
        var result = await _messageBus.InvokeAsync<Result<LoginResponse>>(command);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        SetRefreshTokenCookie(result.Value.RefreshToken);
        return Ok(result.Value);
    }

    [AllowAnonymous]
    [Permission(
        resourceKey: "identity.auth",
        action: "refresh-token",
        description: "Refresh access token",
        recommendedRoles: new[] { "Admin", "User", "Moderator" }
    )]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? body = null)
    {
        return await HandleRefreshTokenAsync(body);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest? body = null)
    {
        return await HandleRefreshTokenAsync(body);
    }

    private async Task<IActionResult> HandleRefreshTokenAsync(RefreshTokenRequest? body)
    {
        var refreshToken = body?.RefreshToken ?? Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { error = "Refresh token is required" });

        var command = new RefreshTokenCommand(refreshToken, GetIpAddress());
        var result = await _messageBus.InvokeAsync<Result<LoginResponse>>(command);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        SetRefreshTokenCookie(result.Value.RefreshToken);
        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailCommand command)
    {
        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Email verified successfully" });
    }


    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerificationCode([FromBody] RegenerateVerificationCodeCommand command)
    {
        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Verification code sent successfully" });
    }

    [Permission(
        resourceKey: "identity.auth",
        action: "change-password",
        description: "Change user password",
        recommendedRoles: new[] { "Admin", "User", "Moderator" }
    )]
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var userId = GetCurrentUserId();
        command = command with { UserId = userId };

        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Password changed successfully" });
    }

    [Permission(
        resourceKey: "identity.auth",
        action: "logout",
        description: "LogOut",
        recommendedRoles: new[] { "Admin", "User", "Moderator" }
    )]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var userId = GetCurrentUserId();
            var command = new RevokeRefreshTokenCommand(userId, refreshToken, GetIpAddress());
            await _messageBus.InvokeAsync<Result>(command);
        }

        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "Logged out successfully" });
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(_currentUserService.UserId!);
    }
}

public record RefreshTokenRequest(string? RefreshToken);