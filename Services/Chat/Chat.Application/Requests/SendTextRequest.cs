using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Requests;

public sealed record SendTextRequest(Guid ConversationId, Guid SenderChatProfileId,
    string Content, Guid? ReplyToMessageId, List<MentionRequestDto>? Mentions);
