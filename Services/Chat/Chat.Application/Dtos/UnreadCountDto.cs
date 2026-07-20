using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Dtos;

public sealed record UnreadCountDto(Guid ConversationId, int UnreadCount);
