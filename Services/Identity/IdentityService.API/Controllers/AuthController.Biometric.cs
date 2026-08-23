using IdentityService.API.Requests;
using IdentityService.Application.Commands;
using IdentityService.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Commons;
using Shared.Security.Attributes;

namespace IdentityService.API.Controllers;

public partial class AuthController
{
    [Permission(
        resourceKey: "identity.auth",
        action: "biometric-enroll",
        description: "Enroll a device for biometric login",
        recommendedRoles: new[] { "Admin", "User", "Moderator" }
    )]
    [HttpPost("biometric/enroll")]
    public async Task<IActionResult> EnrollBiometric(
        [FromBody] EnrollBiometricRequest request,
        CancellationToken cancellationToken)
    {
        var command = new EnrollBiometricCommand(GetCurrentUserId(), request.DeviceId, request.PublicKey);
        var result = await _messageBus.InvokeAsync<Result>(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Biometric enrollment successful" });
    }

    [AllowAnonymous]
    [HttpPost("biometric/challenge")]
    public async Task<IActionResult> RequestBiometricChallenge(
        [FromBody] RequestBiometricChallengeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<BiometricChallengeResponse>>(command, cancellationToken);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpPost("biometric/login")]
    public async Task<IActionResult> BiometricLogin(
        [FromBody] BiometricLoginCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { IpAddress = GetIpAddress() };
        var result = await _messageBus.InvokeAsync<Result<LoginResponse>>(command, cancellationToken);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        SetRefreshTokenCookie(result.Value.RefreshToken);
        return Ok(result.Value);
    }
}