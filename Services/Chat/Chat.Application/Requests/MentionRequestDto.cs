using Chat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Requests;

public sealed record MentionRequestDto(MentionType Type, Guid? ChatProfileId, Guid? MentionGroupId);
