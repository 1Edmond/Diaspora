using Chat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Requests;

public sealed record AddParticipantRequest(Guid RequestingChatProfileId,
    Guid ChatProfileId, ParticipantRole Role = ParticipantRole.Member);
