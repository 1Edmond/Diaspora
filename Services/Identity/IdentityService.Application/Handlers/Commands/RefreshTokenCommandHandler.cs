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

public static class RefreshTokenCommandHandler
{
    public static async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand command,
        IRepository<User> repository,
        IJwtTokenService jwtService,
        IUnitOfWork unitOfWork,
        ILogger<RefreshTokenCommand> logger)
    {
        var users = await repository.GetAllAsync();
        var user = users.FirstOrDefault(u =>
            u.RefreshTokens.Any(t => t.Token == command.RefreshToken));

        if (user == null)
        {
            return Result.Failure<LoginResponse>("Invalid refresh token");
        }

        var refreshToken = user.RefreshTokens.First(t => t.Token == command.RefreshToken);

        if (!refreshToken.IsActive)
        {
            return Result.Failure<LoginResponse>("Invalid refresh token");
        }

        var newRefreshToken = jwtService.GenerateRefreshToken();
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        user.RevokeRefreshToken(command.RefreshToken, command.IpAddress, newRefreshToken);
        user.AddRefreshToken(newRefreshToken, command.IpAddress, newRefreshTokenExpiry);


        user.RemoveOldRefreshTokens(ttlDays: 30);

        await unitOfWork.SaveChangesAsync();

        var accessToken = jwtService.GenerateAccessToken(
            user.Id.ToString(),
            user.Email.Value,
            user.UserRoles.Select(r => r.Role.Name));

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