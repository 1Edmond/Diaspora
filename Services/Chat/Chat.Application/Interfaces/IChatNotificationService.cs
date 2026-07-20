using Chat.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Interfaces;

public interface IChatNotificationService
{
    Task NotifyMessageSent(Guid conversationId, MessageDto message, CancellationToken ct = default);
    Task NotifyMessageEdited(Guid conversationId, Guid messageId, string newContent, CancellationToken ct = default);
    Task NotifyMessageDeleted(Guid conversationId, Guid messageId, CancellationToken ct = default);
    Task NotifyReactionAdded(Guid conversationId, Guid messageId, Guid chatProfileId, string emoji, CancellationToken ct = default);
    Task NotifyParticipantAdded(Guid conversationId, Guid chatProfileId, CancellationToken ct = default);
    Task NotifyCallStarted(Guid conversationId, Guid callId, Guid initiatorChatProfileId, string callType, CancellationToken ct = default);
    Task NotifyCallEnded(Guid conversationId, Guid callId, string status, CancellationToken ct = default);
    Task NotifyProfileStatusChanged(Guid chatProfileId, string status, CancellationToken ct = default);
}