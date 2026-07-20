using Chat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Requests;

public sealed record MentionRequest(
    MentionType Type,
    Guid? ChatProfileId = null,
    Guid? MentionGroupId = null
);
