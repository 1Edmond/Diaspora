using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using IdentityService.Application.Models;
using IdentityService.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public class RequestBiometricChallengeCommandHandler(
    IRepository<User> userRepository,
    IBiometricChallengeStore challengeStore)
{
    private static readonly TimeSpan ChallengeTtl = TimeSpan.FromSeconds(90);

    public async Task<Result<BiometricChallengeResponse>> Handle(
        RequestBiometricChallengeCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync((x) => x.Email.Value.ToLower() == command.Email.ToLower()
        , cancellationToken);

        if (user is null ||
            string.IsNullOrEmpty(user.DeviceId) ||
            string.IsNullOrEmpty(user.DevicePublicKeyId) ||
            user.DeviceId != command.DeviceId)
        {
            return Result.Failure<BiometricChallengeResponse>("Invalid credentials");
        }

        if (user.IsLockedOut())
            return Result.Failure<BiometricChallengeResponse>($"Account locked until {user.LockedUntil}");

        var challenge = await challengeStore.CreateAsync(user.Id, command.DeviceId, ChallengeTtl, cancellationToken);

        return Result.Success(new BiometricChallengeResponse(challenge.Nonce, challenge.ExpiresAt));
    }
}