using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Requests;

public sealed record CreateMentionGroupRequest(Guid RequestingChatProfileId,
    string Name, List<Guid>? MemberChatProfileIds);