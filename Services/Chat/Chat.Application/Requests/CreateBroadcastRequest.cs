using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Requests;

public sealed record CreateBroadcastRequest(string Title, string? Description,
    Guid CreatedByChatProfileId);
