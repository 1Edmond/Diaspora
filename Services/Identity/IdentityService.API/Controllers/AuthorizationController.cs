using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/authorization")]
public class AuthorizationController(
    IPermissionService permissionService,
    ILogger<AuthorizationController> logger,
    ICurrentUserService _currentUserService) : ControllerBase
{
   
    [AllowAnonymous]
    [HttpGet("check")]
    public async Task<IActionResult> CheckPermission(
      [FromQuery] Guid userId,
      [FromQuery] string path,
      [FromQuery] string method)
    {
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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