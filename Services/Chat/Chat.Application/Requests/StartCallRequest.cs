using Chat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Requests;

public sealed record StartCallRequest(Guid ConversationId, Guid InitiatorChatProfileId, CallType Type);