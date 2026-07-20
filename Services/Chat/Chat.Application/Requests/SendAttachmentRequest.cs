using Chat.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Requests;

public sealed record SendAttachmentRequest(Guid ConversationId, Guid SenderChatProfileId,
    MessageType Type, Guid? ReplyToMessageId, IFormFile File);
