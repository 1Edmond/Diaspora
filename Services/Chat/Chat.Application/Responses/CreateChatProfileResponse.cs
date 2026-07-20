using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Responses;

public sealed record CreateChatProfileResponse(Guid ChatProfileId);