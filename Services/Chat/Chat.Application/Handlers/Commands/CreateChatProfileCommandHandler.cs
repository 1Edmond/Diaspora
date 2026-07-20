using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Chat.Application.Responses;
using Chat.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class CreateChatProfileCommandHandler(
    IChatProfileRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<CreateChatProfileResponse>> Handle(
        CreateChatProfileCommand command,
        CancellationToken cancellationToken)
    {
        var exists = await repository.ExistsByUserIdAsync(command.UserId, cancellationToken);
        if (exists)
            return Result.Failure<CreateChatProfileResponse>(
                "A chat profile already exists for this user.");

        var result = ChatProfile.Create(
            command.UserId, command.ExternalProfileId,
            command.DisplayName, command.AvatarUrl,
            command.IsBot, command.BotToken);

        if (result.IsFailure)
            return Result.Failure<CreateChatProfileResponse>(result.Error);

        await repository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateChatProfileResponse(result.Value.Id));
    }
}
