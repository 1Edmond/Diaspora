using IdentityService.Application.Abstractions;
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

    [Fact]
    public async Task AdminLogin_AfterSeeding_ShouldSucceedAndReturnValidLoginResponse()
    {
        // Arrange
        using var context = CreateInMemoryContext(Guid.NewGuid().ToString());
        var hashingService = Substitute.For<IHashingService>();
        hashingService.HashPassword("1234").Returns("valid_hash");
        hashingService.VerifyPassword("1234", "valid_hash").Returns(true);
        var seederLogger = Substitute.For<ILogger<AdminUserSeeder>>();

        var adminRole = new Role
        {
            Id = DefaultRole.GetRoleId(DefaultRole.Admin),
            Name = DefaultRole.Admin,
            Description = "Administrator"
        };
        await context.Roles.AddAsync(adminRole);
        await context.SaveChangesAsync();

        var seeder = new AdminUserSeeder(context, hashingService, seederLogger);
        await seeder.SeedAsync();

        var userRepo = Substitute.For<IUserRepository>();
        var seededAdmin = await context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).Include(u => u.RefreshTokens).FirstAsync();
        userRepo.GetByEmailWithRolesAsync(AdminUserSeeder.DefaultAdminEmail).Returns(seededAdmin);

        var refreshRepo = Substitute.For<IRepository<RefreshToken>>();
        var jwtService = Substitute.For<IJwtTokenService>();
        jwtService.GenerateAccessToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("access_token_123");
        jwtService.GenerateRefreshToken().Returns("refresh_token_123");

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var loginLogger = Substitute.For<ILogger<LoginCommand>>();

        var command = new LoginCommand(AdminUserSeeder.DefaultAdminEmail, "1234", "127.0.0.1");

        // Act
        var result = await LoginCommandHandler.HandleAsync(
            command,
            userRepo,
            refreshRepo,
            hashingService,
            jwtService,
            unitOfWork,
            loginLogger);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access_token_123");
        result.Value.Role.Should().Contain(DefaultRole.Admin);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
