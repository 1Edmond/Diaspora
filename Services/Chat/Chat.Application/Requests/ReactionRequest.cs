using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Requests;

public sealed record ReactionRequest(Guid ChatProfileId, string Emoji);
