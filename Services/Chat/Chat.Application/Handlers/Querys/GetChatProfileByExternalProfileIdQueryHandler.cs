using Chat.Application.Dtos;
using Chat.Application.Extensions;
using Chat.Application.Interfaces;
using Chat.Application.Querys;
using Shared.Domain.Commons;
using System;

namespace Chat.Application.Handlers.Querys;

public sealed class GetChatProfileByExternalProfileIdQueryHandler(IChatProfileRepository repository)
{
    public async Task<Result<ChatProfileDto>> Handle(
        GetChatProfileByExternalProfileIdQuery query, CancellationToken cancellationToken)
    {
        var profile = await repository.GetByProfileIdAsync(query.ExternalProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure<ChatProfileDto>("Chat profile not found.");
        return Result.Success(profile.ToDto());
    }
}