using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Chat.Application.Responses;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class CreateCommunityCommandHandler(
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<CreateCommunityResponse>> Handle(
        CreateCommunityCommand command,
        CancellationToken cancellationToken)
    {
        var result = Domain.Entities.Conversation.CreateCommunity(
            command.Title, command.Description,
            command.AvatarUrl, command.CreatedByChatProfileId);

        if (result.IsFailure)
            return Result.Failure<CreateCommunityResponse>(result.Error);

        await conversationRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateCommunityResponse(result.Value.Id));
    }
}

