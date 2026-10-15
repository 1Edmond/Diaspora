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
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public static class LoginCommandHandler
{
    public static async Task<Result<LoginResponse>> HandleAsync(
        LoginCommand command,
        IRepository<User> repository,
        IRepository<RefreshToken> refreshRepository,
        IHashingService hashingService,
        IJwtTokenService jwtService,
        IUnitOfWork unitOfWork,
        ILogger<LoginCommand> logger)
    {
        var user = await repository.FirstOrDefaultAsync(u => u.Email.Value.ToLower() == command.Email.ToLower());

        if (user == null)
        {
            var delay = TimeSpan.FromSeconds(new Random().Next(1, 5));
            await Task.Delay(delay);
            return Result.Failure<LoginResponse>("Invalid email or password");
        }

        if (user.IsLockedOut())
        {
            return Result.Failure<LoginResponse>($"Account locked until {user.LockedUntil:yyyy-MM-dd HH:mm:ss} UTC");
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

        await repository.UpdateAsync(user);
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
