using IdentityService.Application.Commands;
using IdentityService.Application.Dtos;
using IdentityService.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Security.Attributes;
using Wolverine;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController(IMessageBus _messageBus,
    ILogger<UserController> _logger,  
    ICurrentUserService _currentUserService
    ) : ControllerBase
{
    [Permission(
        resourceKey: "identity.users",
        action: "read",
        description: "Get user by ID",
        recommendedRoles: ["Admin", "User", "Moderator"]
    )]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var query = new GetUserQuery(id);
        var result = await _messageBus.InvokeAsync<Result<UserDto>>(query);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(DataResponse<UserDto>.SuccessResponse(result.Value));
    }

    [Permission(
        resourceKey: "identity.users",
        action: "read-self",
        description: "Get current authenticated user",
        recommendedRoles: ["Admin", "User", "Moderator"]
    )]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var query = new GetUserQuery(userId);
        var result = await _messageBus.InvokeAsync<Result<UserDto>>(query);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(DataResponse<UserDto>.SuccessResponse(result.Value));
    }

    [Permission(
        resourceKey: "identity.users",
        action: "list",
        description: "Get paginated list of users",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
    {
        var result = await _messageBus.InvokeAsync<Result<PagedList<UserDto>>>(query);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "identity.users",
        action: "update",
        description: "Update user profile",
        recommendedRoles: new[] { "Admin", "User", "Moderator" }
    )]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        if (id != currentUserId && !isAdmin)
            return Forbid();

        command = command with { UserId = id };
        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(BaseResponse.SuccessResponse("User updated successfully"));
    }

    [Permission(
        resourceKey: "identity.users",
        action: "suspend",
        description: "Suspend a user account",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPatch("{id:guid}/suspend")]
    public async Task<IActionResult> SuspendUser(Guid id, [FromBody] SuspendUserCommand command)
    {
        command = command with { UserId = id };
        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(BaseResponse.SuccessResponse("User suspended successfully"));
    }

    [Permission(
        resourceKey: "identity.users",
        action: "activate",
        description: "Activate a user account",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        var command = new ActivateUserCommand(id);
        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(BaseResponse.SuccessResponse("User activated successfully"));
    }

    [Permission(
        resourceKey: "identity.users",
        action: "delete",
        description: "Soft delete a user account",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var deletedBy = GetCurrentUserId().ToString();
        var command = new DeleteUserCommand(id, deletedBy);
        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(BaseResponse.SuccessResponse("User deleted successfully"));
    }

    [Permission(
        resourceKey: "identity.users",
        action: "restore",
        description: "Restore a deleted user account",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> RestoreUser(Guid id)
    {
        var command = new RestoreUserCommand(id);
        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(BaseResponse.SuccessResponse("User restored successfully"));
    }

    [Permission(
        resourceKey: "identity.users.tokens",
        action: "read",
        description: "Get refresh tokens for a user",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpGet("{id:guid}/refresh-tokens")]
    public async Task<IActionResult> GetUserRefreshTokens(Guid id)
    {
        var query = new GetUserRefreshTokensQuery(id);
        var result = await _messageBus.InvokeAsync<Result<List<RefreshTokenDto>>>(query);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(DataResponse<List<RefreshTokenDto>>.SuccessResponse(result.Value));
    }

    [Permission(
        resourceKey: "identity.users.tokens",
        action: "revoke-all",
        description: "Revoke all refresh tokens for a user",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{id:guid}/revoke-all-tokens")]
    public async Task<IActionResult> RevokeAllRefreshTokens(Guid id)
    {
        var ipAddress = GetIpAddress();
        var command = new RevokeAllRefreshTokensCommand(id, ipAddress);
        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(BaseResponse.SuccessResponse("All refresh tokens revoked successfully"));
    }

    [Permission(
        resourceKey: "identity.users.kyc",
        action: "read-by-level",
        description: "Get users filtered by KYC level",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpGet("kyc-level/{level}")]
    public async Task<IActionResult> GetUsersByKYCLevel(string level, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (!Enum.TryParse<KYCLevel>(level, true, out var kycLevel))
            return BadRequest(new { error = "Invalid KYC level" });

        var query = new GetUsersByKYCLevelQuery(kycLevel, pageNumber, pageSize);
        var result = await _messageBus.InvokeAsync<Result<PagedList<UserDto>>>(query);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "identity.users.kyc",
        action: "statistics",
        description: "Get KYC statistics across all users",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpGet("kyc-statistics")]
    public async Task<IActionResult> GetKYCStatistics()
    {
        var query = new GetKYCStatisticsQuery();
        var result = await _messageBus.InvokeAsync<Result<KYCStatisticsDto>>(query);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(DataResponse<KYCStatisticsDto>.SuccessResponse(result.Value));
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