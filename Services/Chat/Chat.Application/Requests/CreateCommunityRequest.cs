using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Requests;

public sealed record CreateCommunityRequest(string Title, string? Description, string? AvatarUrl,
    Guid CreatedByChatProfileId);
