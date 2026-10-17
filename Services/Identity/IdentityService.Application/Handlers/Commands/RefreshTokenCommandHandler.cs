using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using IdentityService.Application.Dtos;
using IdentityService.Application.Models;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public static class RefreshTokenCommandHandler
{
    public static async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand command,
        IUserRepository userRepository,
        IRepository<RefreshToken> refreshRepository,
        IJwtTokenService jwtService,
        IUnitOfWork unitOfWork,
        ILogger<RefreshTokenCommand> logger,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByRefreshTokenAsync(command.RefreshToken, cancellationToken);

        if (user == null)
        {
            return Result.Failure<LoginResponse>("Invalid refresh token");
        }

        if (user.Status != UserStatus.Active || user.IsLockedOut())
        {
            return Result.Failure<LoginResponse>("User account is not active or is locked out");
        }

        var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == command.RefreshToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return Result.Failure<LoginResponse>("Invalid refresh token");
        }

        var newRefreshToken = jwtService.GenerateRefreshToken();
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        user.RevokeRefreshToken(command.RefreshToken, command.IpAddress, newRefreshToken);
        var addedToken = user.AddRefreshToken(newRefreshToken, command.IpAddress, newRefreshTokenExpiry);

        user.RemoveOldRefreshTokens(ttlDays: 30);

        await refreshRepository.AddAsync(addedToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = jwtService.GenerateAccessToken(
            user.Id.ToString(),
            user.Email.Value,
            user.UserRoles.Select(r => r.Role.Name),
            user.DataLanguage,
            user.InterfaceLanguage);

        logger.LogInformation("Token refreshed for user: {UserId}", user.Id);

        return Result.Success(new LoginResponse(
            user.Id,
            user.ToDto(),
            [.. user.UserRoles.Select(r => r.Role.Name)],
            accessToken,
            newRefreshToken,
            newRefreshTokenExpiry));
    }
}