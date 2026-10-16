using IdentityService.Application.Commands;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Domain.Commons;
using Shared.Domain.Constants;
using Shared.Domain.Interfaces;
using Shared.Security.Abstractions;

namespace IdentityService.Application.Handlers.Commands;

public class RegisterUserCommandHandler(
    IRepository<User> repository,
    IRepository<Role> roleRepository,
    IUnitOfWork unitOfWork,
    IHashingService hashingService,
    ILogger<RegisterUserCommandHandler> logger)
{
    public async Task<Result<Guid>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var existingUser = await repository.FirstOrDefaultAsync(
            u => u.Email.Value.ToLower() == command.Email.ToLower(),
            cancellationToken);

        if (existingUser != null)
            return Result.Failure<Guid>("Email already registered");

        var passwordHash = hashingService.HashPassword(command.Password);

        var userResult = User.Create(
            command.Email,
            passwordHash,
            command.DateOfBirth,
            command.FirstName,
            command.LastName,
            command.PhoneNumber
            );

        if (userResult.IsFailure)
            return Result.Failure<Guid>(userResult.Error);

        var user = userResult.Value;

        // STRICT SECURITY: Public registration is strictly locked to DefaultRole.User.
        // Any role requested by the caller is ignored to prevent privilege escalation.
        if (command.Role != null && command.Role.Any(r => !string.Equals(r, DefaultRole.User, StringComparison.OrdinalIgnoreCase)))
        {
            logger.LogWarning(
                "[RegisterUser] Public registration attempt specified unauthorized role(s): {Roles}. Overridden to User.",
                string.Join(", ", command.Role));
        }

        var roleName = DefaultRole.User;
        var role = await roleRepository.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
        if (role == null)
        {
            role = await roleRepository.GetByIdAsync(
                Guid.Parse(DefaultRole.GetRoleId(DefaultRole.User).ToString()),
                cancellationToken);
        }

        if (role == null)
        {
            logger.LogError("[RegisterUser] Default role not found in database");
            return Result.Failure<Guid>("System error: Default role not configured");
        }

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow,
        };

        user.UserRoles.Add(userRole);

        await repository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if(logger.IsEnabled(LogLevel.Information))
        logger.LogInformation(
            "[RegisterUser] User created: {UserId}, Role: {Role}",
            user.Id,
            role.Name);

        return Result.Success(user.Id);
    }
}