using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Security.Attributes;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/authorization")]
public class AuthorizationController(
    IPermissionService permissionService,
    ILogger<AuthorizationController> logger,
    ICurrentUserService _currentUserService,
    IConfiguration _configuration) : ControllerBase
{
    private bool IsAuthorizedInternalOrSelfOrAdmin(Guid targetUserId)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
                return true;

            return Guid.TryParse(_currentUserService.UserId, out var currentUserId)
                && currentUserId == targetUserId;
        }

        // Internal Gateway-to-Identity call (no X-User-Id header injected by external proxy)
        var hasExternalUserHeader = Request.Headers.ContainsKey("X-User-Id");
        if (hasExternalUserHeader)
            return false;

        var configuredSecret = _configuration["Gateway:Secret"];
        var requestSecret = Request.Headers["X-Gateway-Secret"].ToString();
        if (!string.IsNullOrEmpty(configuredSecret) && !string.IsNullOrEmpty(requestSecret))
        {
            return string.Equals(configuredSecret, requestSecret, StringComparison.Ordinal);
        }

        // Allow internal cluster calls when X-User-Id is absent
        return true;
    }

    [AllowAnonymous]
    [HttpGet("check")]
    public async Task<IActionResult> CheckPermission(
      [FromQuery] Guid userId,
      [FromQuery] string path,
      [FromQuery] string method)
    {
        if (!IsAuthorizedInternalOrSelfOrAdmin(userId))
            return Forbid();

        var hasPermission = await permissionService.UserHasPermissionAsync(
            userId, path, method
        );

        return Ok(new { HasPermission = hasPermission });
    }


    [AllowAnonymous]
    [HttpGet("user-permissions")]
    public async Task<IActionResult> GetUserPermissions(
      [FromQuery] Guid userId)
    {
        if (!IsAuthorizedInternalOrSelfOrAdmin(userId))
            return Forbid();

        var userAuthorisation = await permissionService.GetUserPermissionsAsync(
            userId);

        if (userAuthorisation == null) 
            return NoContent();

        return Ok(new { 
            HasPermission = true,
            userAuthorisation.Permissions,
            userAuthorisation.Roles,
        });
    }

    [HttpGet("permissions")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [Permission(resourceKey: "authorization.permissions", action: "read", description: "Consulter toutes les permissions système", recommendedRoles: new[] { "Admin" })]
    public async Task<IActionResult> GetAllPermissions()
    {
        var permissions = await permissionService.GetAllPermissionsAsync();

        return Ok(new
        {
            TotalCount = permissions.Count,
            Permissions = permissions.Select(p => new
            {
                p.PermissionKey,
                p.ServiceName,
                p.ResourceName,
                p.Permission,
                p.Description
            })
        });
    }

    [HttpPost("roles/{roleId:guid}/permissions")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [Permission(resourceKey: "authorization.roles.permissions", action: "manage", description: "Assigner des permissions à un rôle", recommendedRoles: new[] { "Admin" })]
    public async Task<IActionResult> AssignPermissionToRole(
        Guid roleId,
        [FromBody] AssignPermissionsCommand request)
    {
        var currentUserId = GetCurrentUserId();

        await permissionService.AssignPermissionsToRoleAsync(
            roleId,
            request.PermissionKeys,
            currentUserId
        );

        return Ok(new { Message = "Permission assigned successfully" });
    }

    [HttpDelete("roles/{roleId:guid}/permissions/{permissionKey}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [Permission(resourceKey: "authorization.roles.permissions", action: "manage", description: "Retirer une permission d'un rôle", recommendedRoles: new[] { "Admin" })]
    public async Task<IActionResult> RemovePermissionFromRole(
        Guid roleId,
        string permissionKey)
    {
        await permissionService.RemovePermissionFromRoleAsync(roleId, permissionKey);

        return Ok(new { Message = "Permission removed successfully" });
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(_currentUserService.UserId!);
    }
}