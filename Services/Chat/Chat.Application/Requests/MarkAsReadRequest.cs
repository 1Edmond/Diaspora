using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Requests;

public sealed record MarkAsReadRequest(Guid ChatProfileId);
