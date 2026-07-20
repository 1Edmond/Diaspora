using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Chat.Application.Responses;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class CreateBroadcastCommandHandler(
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<CreateBroadcastResponse>> Handle(
        CreateBroadcastCommand command,
        CancellationToken cancellationToken)
    {
        var result = Domain.Entities.Conversation.CreateBroadcast(
            command.Title, command.Description, command.CreatedByChatProfileId);

        if (result.IsFailure)
            return Result.Failure<CreateBroadcastResponse>(result.Error);

        await conversationRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateBroadcastResponse(result.Value.Id));
    }
}
