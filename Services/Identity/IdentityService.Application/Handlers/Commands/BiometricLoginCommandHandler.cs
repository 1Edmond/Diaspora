using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using IdentityService.Application.Dtos;
using IdentityService.Application.Models;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine.Persistence;

namespace IdentityService.Application.Handlers.Commands;

public class BiometricLoginCommandHandler(
    IUnitOfWork _unitOfWork,
    IRepository<User> _userRepository,
    IJwtTokenService _jwtService,
    IRepository<RefreshToken> _refreshRepository,
    IBiometricChallengeStore _challengeStore,
    ILogger<LoginCommand> _logger,
    IEd25519SignatureVerifier _signatureVerifier)
{

    public async Task<Result<LoginResponse>> Handle(BiometricLoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FirstOrDefaultAsync((x) => x.Email.Value.ToLower() == command.Email.ToLower()
       , cancellationToken);
        if (user is null || user.DeviceId != command.DeviceId || string.IsNullOrEmpty(user.DevicePublicKeyId))
            return Result.Failure<LoginResponse>("Invalid credentials");

        if (user.IsLockedOut())
            return Result.Failure<LoginResponse>($"Account locked until {user.LockedUntil}");

        var challenge = await _challengeStore.ConsumeAsync(user.Id, command.DeviceId, command.Nonce, cancellationToken);
        if (challenge is null)
            return Result.Failure<LoginResponse>("Challenge expired or already used, please try again");

        var isValidSignature = _signatureVerifier.Verify(user.DevicePublicKeyId, command.Nonce, command.Signature);
        if (!isValidSignature)
        {
            user.RecordFailedLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<LoginResponse>("Invalid credentials");
        }

        var loginResult = user.RecordSuccessfulLogin();
        if (loginResult.IsFailure)
            return Result.Failure<LoginResponse>(loginResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(
            user.Id.ToString(),
            user.Email.Value,
           user.UserRoles.Select(r => r.Role.Name));

        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        user.AddRefreshToken(refreshToken, command.IpAddress, refreshTokenExpiry);
        user.RemoveOldRefreshTokens(ttlDays: 30);

        await _userRepository.UpdateAsync(user);
        await _refreshRepository.AddAsync(user.RefreshTokens.First(
            x => x.Token == refreshToken
            ));
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {UserId} ({Email}) logged in successfully",
            user.Id, user.Email.Value);

        return Result.Success(new LoginResponse(
            user.Id,
            user.ToDto(),
            [.. user.UserRoles.Select(r => r.Role.Name)],
            accessToken,
            refreshToken,
            refreshTokenExpiry));
    }
}