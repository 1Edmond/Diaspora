using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Chat.Application.Responses;
using Chat.Domain.Entities;
using Chat.Domain.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class SendMessageCommandHandler(
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork)
{
    public const string FileEntityType = "chat-attachment";

    public async Task<Result<SendMessageResponse>> Handle(
        SendMessageCommand command,
        CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetWithParticipantsAsync(
            command.ConversationId, cancellationToken);

        if (conversation is null || !conversation.IsActive)
            return Result.Failure<SendMessageResponse>("Conversation not found.");

        // Broadcast : seuls les admins/owners peuvent envoyer
        if (conversation.Type == ConversationType.Broadcast
            && !conversation.IsAdmin(command.SenderChatProfileId))
            return Result.Failure<SendMessageResponse>(
                "Only admins can send messages in a broadcast channel.");

        if (!conversation.HasParticipant(command.SenderChatProfileId))
            return Result.Failure<SendMessageResponse>(
                "You are not a participant of this conversation.");

        var msgResult = Message.Create(
            command.ConversationId, command.SenderChatProfileId,
            command.Type, command.Content, command.ReplyToMessageId);

        if (msgResult.IsFailure)
            return Result.Failure<SendMessageResponse>(msgResult.Error);

        var message = msgResult.Value;

        // ── Attachment ────────────────────────────────────────────────────────
        if (command.Attachment is not null)
        {
            await using var stream = command.Attachment.OpenReadStream();
            var uploadResult = await fileStorageService.UploadEncryptedFileAsync(
                stream, command.Attachment.FileName, command.Attachment.ContentType,
                entityId: command.ConversationId.ToString(),
                entityType: FileEntityType, cancellationToken);

            if (uploadResult.IsFailure)
                return Result.Failure<SendMessageResponse>(uploadResult.Error);

            var attachType = command.Type switch
            {
                MessageType.Image => AttachmentType.Image,
                MessageType.Voice => AttachmentType.Voice,
                _ => AttachmentType.File
            };

            message.AddAttachment(attachType,
                command.Attachment.FileName, uploadResult.Value,
                command.Attachment.ContentType, command.Attachment.Length);
        }

        // ── Mentions ──────────────────────────────────────────────────────────
        foreach (var mention in command.Mentions)
        {
            switch (mention.Type)
            {
                case MentionType.Profile when mention.ChatProfileId.HasValue:
                    message.AddMentionProfile(mention.ChatProfileId.Value);
                    break;
                case MentionType.Everyone:
                    message.AddMentionEveryone();
                    break;
                case MentionType.Group when mention.MentionGroupId.HasValue:
                    message.AddMentionGroup(mention.MentionGroupId.Value);
                    break;
            }
        }

        await messageRepository.AddAsync(message, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new SendMessageResponse(message.Id, message.CreatedAt));
    }
}
