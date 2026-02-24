using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Domain.Constants;
using Shared.Security.Attributes;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/roles")]
public class RoleController(
    ICurrentUserService _currentUserService,
    IRoleRepository roleRepository,
    IResourcePermissionRepository resourcePermissionRepository,
    ILogger<RoleController> logger) : ControllerBase
{
   
    [Permission(
        resourceKey: "identity.roles",
        action: "read",
        description: "Get all roles",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await roleRepository.GetAllAsync();

        return Ok(roles.Select(r => new
        {
            r.Id,
            r.Name,
            r.Description,
            r.CreatedAt,
            PermissionCount = r.RolePermissions.Count
        }));
    }

    [Permission(
        resourceKey: "identity.roles",
        action: "read",
        description: "Get role by ID",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRole(Guid id)
    {
        var role = await roleRepository.GetByIdAsync(id);

        if (role == null)
            return NotFound(new { Error = "Role not found" });

        return Ok(new
        {
            role.Id,
            role.Name,
            role.Description,
            role.CreatedAt,
            Permissions = role.RolePermissions.Select(rp => new
            {
                rp.PermissionKey,
                rp.GrantedAt
            })
        });
    }

    [Permission(
        resourceKey: "identity.roles",
        action: "create",
        description: "Create a new role",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Creating role: {Name}", command.Name);

        var newRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            CreatedAt = DateTime.UtcNow
        };

        await roleRepository.AddAsync(newRole);

        logger.LogInformation("✅ Role created: {RoleId}", newRole.Id);

        return CreatedAtAction(
            nameof(GetRole),
            new { id = newRole.Id },
            new { newRole.Id, newRole.Name }
        );
    }

    [Permission(
        resourceKey: "identity.roles",
        action: "update",
        description: "Update an existing role",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleCommand command)
    {
        var role = await roleRepository.GetByIdAsync(id);

        if (role == null)
            return NotFound(new { Error = "Role not found" });

        role.Name = command.Name ?? role.Name;
        role.Description = command.Description ?? role.Description;

        await roleRepository.UpdateAsync(role);

        return Ok(new { Message = "Role updated successfully" });
    }

    [Permission(
        resourceKey: "identity.roles",
        action: "delete",
        description: "Delete a role",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        if (!String.IsNullOrEmpty(DefaultRole.GetRoleName(id)))
        {
            return BadRequest(new { Error = "Cannot delete default roles" });
        }

        await roleRepository.DeleteAsync(id);

        return Ok(new { Message = "Role deleted successfully" });
    }

    [Permission(
        resourceKey: "identity.roles.permissions",
        action: "assign",
        description: "Assign permissions to a role",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{id:guid}/permissions")]
    public async Task<IActionResult> AssignPermissions(
        Guid id,
        [FromBody] AssignPermissionsCommand command)
    {
        var role = await roleRepository.GetByIdAsync(id);

        if (role == null)
            return NotFound(new { Error = "Role not found" });

        var currentUserId = GetCurrentUserId();
        var assignedCount = 0;

        foreach (var permissionKey in command.PermissionKeys)
        {
            var permission = await resourcePermissionRepository.GetByPermissionKeyAsync(permissionKey);

            if (permission == null)
            {
                logger.LogWarning(
                    "[RoleController] Permission not found: {Permission}",
                    permissionKey
                );
                continue;
            }

            await roleRepository.AddPermissionToRoleAsync(
                id,
                permissionKey,
                currentUserId
            );
            assignedCount++;
        }

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "✅ Assigned {Count} permissions to role {RoleName}",
                assignedCount,
                role.Name
            );

        return Ok(new
        {
            Message = $"{assignedCount} permissions assigned",
            AssignedCount = assignedCount,
            RequestedCount = command.PermissionKeys.Count
        });
    }

    [Permission(
        resourceKey: "identity.roles.permissions",
        action: "remove",
        description: "Remove permissions from a role",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpDelete("{id:guid}/permissions")]
    public async Task<IActionResult> RemovePermissions(
        Guid id,
        [FromBody] RemovePermissionsCommand command)
    {
        var role = await roleRepository.GetByIdAsync(id);

        if (role == null)
            return NotFound(new { Error = "Role not found" });

        var removedCount = 0;

        foreach (var permissionKey in command.PermissionKeys)
        {
            await roleRepository.RemovePermissionFromRoleAsync(id, permissionKey);
            removedCount++;
        }

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "✅ Removed {Count} permissions from role {RoleName}",
                removedCount,
                role.Name
            );

        return Ok(new
        {
            Message = $"{removedCount} permissions removed",
            RemovedCount = removedCount
        });
    }

    [Permission(
        resourceKey: "identity.roles.permissions",
        action: "read",
        description: "Get all available permissions",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpGet("available-permissions")]
    public async Task<IActionResult> GetAvailablePermissions()
    {
        var permissions = await resourcePermissionRepository.GetAllActiveAsync();

        var grouped = permissions
            .GroupBy(p => p.ServiceName)
            .Select(g => new
            {
                ServiceName = g.Key,
                Resources = g.GroupBy(p => p.ResourceName)
                    .Select(r => new
                    {
                        ResourceName = r.Key,
                        r.First().ResourceKey,
                        Permissions = r.Select(p => new
                        {
                            p.PermissionKey,
                            p.Permission,
                            p.Description
                        })
                    })
            });

        return Ok(grouped);
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(_currentUserService.UserId!);
    }
}