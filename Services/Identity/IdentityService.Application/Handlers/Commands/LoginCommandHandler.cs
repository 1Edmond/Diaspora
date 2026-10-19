using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using IdentityService.Application.Dtos;
using IdentityService.Application.Models;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public static class LoginCommandHandler
{
    public static async Task<Result<LoginResponse>> HandleAsync(
        LoginCommand command,
        IUserRepository userRepository,
        IRepository<RefreshToken> refreshRepository,
        IHashingService hashingService,
        IJwtTokenService jwtService,
        IUnitOfWork unitOfWork,
        ILogger<LoginCommand> logger)
    {
        var user = await userRepository.GetByEmailWithRolesAsync(command.Email);

        if (user == null)
        {
            // Égalisation temporelle contre l'énumération d'adresses email (timing attack mitigation)
            hashingService.VerifyPassword(command.Password, "$argon2id$v=19$m=65536,t=3,p=1$c29tZXNhbHQ$DummyHashToPreventTimingAttack1234567890123456");
            return Result.Failure<LoginResponse>("Invalid email or password");
        }

        if (user.IsLockedOut())
        {
            return Result.Failure<LoginResponse>($"Account locked until {user.LockedUntil:yyyy-MM-dd HH:mm:ss} UTC");
        }

        if (user.IsDeleted || user.Status != Shared.Domain.Enums.UserStatus.Active)
        {
            return Result.Failure<LoginResponse>($"User account is {user.Status}");
        }

        if (!hashingService.VerifyPassword(command.Password, user.PasswordHash))
        {
            var failureResult = user.RecordFailedLogin();
            if (failureResult.IsFailure)
            {
                return Result.Failure<LoginResponse>(failureResult.Error);
            }
            await unitOfWork.SaveChangesAsync();
            return Result.Failure<LoginResponse>("Invalid email or password");
        }

        if (!user.EmailVerified)
        {
            return Result.Failure<LoginResponse>("Email not verified. Please check your inbox.");
        }

        var loginResult = user.RecordSuccessfulLogin();
        if (loginResult.IsFailure)
        {
            return Result.Failure<LoginResponse>(loginResult.Error);
        }


        var accessToken = jwtService.GenerateAccessToken(
            user.Id.ToString(),
            user.Email.Value,
           user.UserRoles.Select(r => r.Role.Name),
           user.DataLanguage,
           user.InterfaceLanguage);

        var refreshToken = jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        user.RevokeAllActiveTokens(command.IpAddress, "Single session policy: superseded by new login");
        user.AddRefreshToken(refreshToken, command.IpAddress, refreshTokenExpiry);
        user.RemoveOldRefreshTokens(ttlDays: 30);

        await userRepository.UpdateAsync(user);
        await refreshRepository.AddAsync(user.RefreshTokens.First(
            x => x.Token == refreshToken
            ));
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("User {UserId} ({Email}) logged in successfully",
            user.Id, user.Email.Value);

        return Result.Success(new LoginResponse(
            user.Id,
            user.ToDto(),
            [..user.UserRoles.Select(r => r.Role.Name)],
            accessToken,
            refreshToken,
            refreshTokenExpiry));
    }
}
