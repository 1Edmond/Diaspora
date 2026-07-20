using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Responses;

public sealed record CreateMentionGroupResponse(Guid MentionGroupId);