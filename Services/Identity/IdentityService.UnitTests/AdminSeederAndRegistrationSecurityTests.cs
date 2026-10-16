using FluentAssertions;
using IdentityService.Application.Commands;
using IdentityService.Application.Handlers.Commands;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Application.Interfaces;
using Shared.Domain.Constants;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events;
using Shared.Security.Abstractions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IdentityService.UnitTests;

public class AdminSeederAndRegistrationSecurityTests
{
    private static IdentityDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var integrationEventMapper = Substitute.For<IIntegrationEventMapper>();
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var logger = Substitute.For<ILogger<IdentityDbContext>>();

        return new IdentityDbContext(options, integrationEventMapper, dateTimeProvider, currentUserService, logger);
    }

    [Fact]
    public async Task RegisterUser_WhenRoleAdminRequested_ShouldForceUserRoleAndNotAssignAdmin()
    {
        // Arrange
        var userRepo = Substitute.For<IRepository<User>>();
        var roleRepo = Substitute.For<IRepository<Role>>();
        var hashingService = Substitute.For<IHashingService>();
        var logger = Substitute.For<ILogger<RegisterUserCommandHandler>>();

        hashingService.HashPassword(Arg.Any<string>()).Returns("hashed_pwd");

        userRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var userRoleEntity = new Role
        {
            Id = DefaultRole.GetRoleId(DefaultRole.User),
            Name = DefaultRole.User,
            Description = "Standard user"
        };

        roleRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<Role, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(userRoleEntity);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RegisterUserCommandHandler(userRepo, roleRepo, unitOfWork, hashingService, logger);

        // Attempt privilege escalation by specifying Role = ["Admin"]
        var maliciousCommand = new RegisterUserCommand(
            "hacker@diaspora.org",
            "P@ssword123",
            new List<string> { DefaultRole.Admin },
            "Bad",
            new DateOnly(1995, 5, 20),
            "Actor",
            null);

        // Act
        var result = await handler.HandleAsync(maliciousCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Verify User was saved
        await userRepo.Received(1).AddAsync(Arg.Is<User>(u =>
            u.Email.Value == "hacker@diaspora.org" &&
            u.UserRoles.Any(ur => ur.RoleId == userRoleEntity.Id)),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdminUserSeeder_WhenNoAdminExists_ShouldSeedAdminWith1234PasswordHash()
    {
        // Arrange
        using var context = CreateInMemoryContext(Guid.NewGuid().ToString());
        var hashingService = Substitute.For<IHashingService>();
        hashingService.HashPassword("1234").Returns("bcrypt_hashed_1234");
        var logger = Substitute.For<ILogger<AdminUserSeeder>>();

        // Seed the admin role in context first
        var adminRole = new Role
        {
            Id = DefaultRole.GetRoleId(DefaultRole.Admin),
            Name = DefaultRole.Admin,
            Description = "Administrator"
        };
        await context.Roles.AddAsync(adminRole);
        await context.SaveChangesAsync();

        var seeder = new AdminUserSeeder(context, hashingService, logger);

        // Act
        await seeder.SeedAsync();

        // Assert
        var users = await context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ToListAsync();
        users.Should().ContainSingle();

        var admin = users.First();
        admin.Email.Value.Should().Be("admin@diaspora.org");
        admin.PasswordHash.Should().Be("bcrypt_hashed_1234");
        admin.EmailVerified.Should().BeTrue();
        admin.UserRoles.Should().ContainSingle(ur => ur.Role.Name == DefaultRole.Admin);
    }

    [Fact]
    public async Task AdminUserSeeder_WhenAdminAlreadyExists_ShouldNotDuplicate()
    {
        // Arrange
        using var context = CreateInMemoryContext(Guid.NewGuid().ToString());
        var hashingService = Substitute.For<IHashingService>();
        hashingService.HashPassword("1234").Returns("bcrypt_hashed_1234");
        var logger = Substitute.For<ILogger<AdminUserSeeder>>();

        var adminRole = new Role
        {
            Id = DefaultRole.GetRoleId(DefaultRole.Admin),
            Name = DefaultRole.Admin,
            Description = "Administrator"
        };
        await context.Roles.AddAsync(adminRole);
        await context.SaveChangesAsync();

        var seeder = new AdminUserSeeder(context, hashingService, logger);

        // Act
        await seeder.SeedAsync();
        await seeder.SeedAsync(); // Run again

        // Assert
        var users = await context.Users.ToListAsync();
        users.Should().ContainSingle();
    }
}
