using Chat.Application.Requests;
using Chat.Application.Responses;
using Chat.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Commands;

public sealed record SendMessageCommand(
    Guid ConversationId,
    Guid SenderChatProfileId,
    MessageType Type,
    string? Content,
    Guid? ReplyToMessageId,
    List<MentionRequest> Mentions,
    IFormFile? Attachment = null
) : ICommand<Result<SendMessageResponse>>;