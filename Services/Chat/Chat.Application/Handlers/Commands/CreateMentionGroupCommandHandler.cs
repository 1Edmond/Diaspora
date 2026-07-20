using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Chat.Application.Responses;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class CreateMentionGroupCommandHandler(
    IConversationRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<CreateMentionGroupResponse>> Handle(
        CreateMentionGroupCommand command,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetWithParticipantsAsync(
            command.ConversationId, cancellationToken);

        if (conversation is null)
            return Result.Failure<CreateMentionGroupResponse>("Conversation not found.");

        if (!conversation.IsAdmin(command.RequestingChatProfileId))
            return Result.Failure<CreateMentionGroupResponse>(
                "Only admins can create mention groups.");

        var result = conversation.AddMentionGroup(command.Name);
        if (result.IsFailure)
            return Result.Failure<CreateMentionGroupResponse>(result.Error);

        foreach (var memberId in command.MemberChatProfileIds)
            result.Value.AddMember(memberId);

        await repository.UpdateAsync(conversation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateMentionGroupResponse(result.Value.Id));
    }
}