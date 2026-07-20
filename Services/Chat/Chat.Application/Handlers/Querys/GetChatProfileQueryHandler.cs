using Chat.Application.Dtos;
using Chat.Application.Extensions;
using Chat.Application.Interfaces;
using Chat.Application.Querys;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Querys;

public sealed class GetChatProfileQueryHandler(IChatProfileRepository repository)
{
    public async Task<Result<ChatProfileDto>> Handle(
        GetChatProfileQuery query, CancellationToken cancellationToken)
    {
        var profile = await repository.GetByIdAsync(query.ChatProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure<ChatProfileDto>("Chat profile not found.");
        return Result.Success(profile.ToDto());
    }
}