using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Domain.Constants;
using Shared.Security.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Infrastructure.Persistence;

public class AdminUserSeeder(
    IdentityDbContext _context,
    IHashingService _hashingService,
    ILogger<AdminUserSeeder> _logger)
{
    public const string DefaultAdminEmail = "admin@diaspora.org";
    public const string DefaultAdminPassword = "1234";

    public async Task SeedAsync()
    {
        await EnsureAdminRoleHasAllPermissionsAsync();

        var anyAdminExists = await _context.Users
            .AnyAsync(u => u.UserRoles.Any(ur => ur.Role.Name == DefaultRole.Admin));

        if (anyAdminExists)
        {
            _logger.LogInformation("[AdminUserSeeder] Admin user already exists, skipping");
            return;
        }

        _logger.LogInformation("[AdminUserSeeder] Seeding default admin user: {Email}...", DefaultAdminEmail);

        var adminRole = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Name == DefaultRole.Admin)
            ?? await _context.Roles.FindAsync(DefaultRole.GetRoleId(DefaultRole.Admin));

        if (adminRole == null)
        {
            _logger.LogError("[AdminUserSeeder] Cannot seed admin user: Admin role does not exist in database");
            return;
        }

        var passwordHash = _hashingService.HashPassword(DefaultAdminPassword);

        var userResult = User.Create(
            DefaultAdminEmail,
            passwordHash,
            new DateOnly(1990, 1, 1),
            "Super",
            "Admin",
            "+22890000000");

        if (userResult.IsFailure)
        {
            _logger.LogError("[AdminUserSeeder] Failed to create Admin user entity: {Error}", userResult.Error);
            return;
        }

        var adminUser = userResult.Value;
        adminUser.Activate();
        if (!string.IsNullOrEmpty(adminUser.EmailVerificationCode))
        {
            adminUser.VerifyEmail(adminUser.EmailVerificationCode);
        }

        await _context.Users.AddAsync(adminUser);

        var userRole = new UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = "system"
        };
        await _context.UserRoles.AddAsync(userRole);

        await _context.SaveChangesAsync();
        _logger.LogInformation("[AdminUserSeeder] Default Admin user created successfully: {Email} with role {Role}", DefaultAdminEmail, DefaultRole.Admin);
    }

    private async Task EnsureAdminRoleHasAllPermissionsAsync()
    {
        var adminRoles = await _context.Roles
            .Include(r => r.RolePermissions)
            .Where(r => r.Name == DefaultRole.Admin || r.Name == "SuperAdmin")
            .ToListAsync();

        if (adminRoles.Count == 0)
            return;

        var allPermissions = await _context.ResourcePermissions
            .Where(p => p.IsActive)
            .ToListAsync();

        var addedCount = 0;
        foreach (var role in adminRoles)
        {
            foreach (var permission in allPermissions)
            {
                if (!role.RolePermissions.Any(rp =>
                        rp.PermissionKey.Equals(permission.PermissionKey, StringComparison.OrdinalIgnoreCase)))
                {
                    await _context.RolePermissions.AddAsync(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionKey = permission.PermissionKey,
                        GrantedAt = DateTime.UtcNow
                    });
                    addedCount++;
                }
            }
        }

        if (addedCount > 0)
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("[AdminUserSeeder] Synchronized {Count} missing permissions to Admin role(s)", addedCount);
        }
    }
}
