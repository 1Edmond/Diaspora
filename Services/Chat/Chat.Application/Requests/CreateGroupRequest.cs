using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Requests;

public sealed record CreateGroupRequest(string Title, string? Description, string? AvatarUrl,
    Guid CreatedByChatProfileId, List<Guid>? InitialParticipantIds);
