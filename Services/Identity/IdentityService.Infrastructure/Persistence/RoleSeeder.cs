using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Domain.Constants;

namespace IdentityService.Infrastructure.Seed;

public class RoleSeeder(IdentityDbContext context, ILogger<RoleSeeder> logger)
{
    private readonly IdentityDbContext _context = context;
    private readonly ILogger<RoleSeeder> _logger = logger;

    public async Task SeedAsync()
    {
        var anyRoleExists = await _context.Roles.AnyAsync();
        if (anyRoleExists)
        {
            _logger.LogInformation("[RoleSeeder] Roles already exist, skipping");
            return;
        }

        _logger.LogInformation("[RoleSeeder] Seeding default roles...");

        var roles = new List<Role>
        {
            new()
            {
                Id = DefaultRole.GetRoleId(DefaultRole.Admin),
                Name = DefaultRole.Admin,
                Description = "Administrator with full access",
                CreatedAt = DateTime.UtcNow, 
                CreatedBy = "system"
            },
            new()
            {
                Id = DefaultRole.GetRoleId(DefaultRole.User),
                Name = DefaultRole.User,
                Description = "Standard user with limited access",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = DefaultRole.GetRoleId(DefaultRole.Moderator),
                Name = DefaultRole.Moderator,
                Description = "Moderator with content management permissions",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            }
        };

        await _context.Roles.AddRangeAsync(roles);
        await _context.SaveChangesAsync();

        _logger.LogInformation("[RoleSeeder] ✅ {Count} default roles seeded", roles.Count);
    }
}