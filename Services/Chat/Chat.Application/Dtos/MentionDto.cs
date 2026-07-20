using Chat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Dtos;

public sealed record MentionDto(
    MentionType Type,
    Guid? ChatProfileId,
    Guid? MentionGroupId
);