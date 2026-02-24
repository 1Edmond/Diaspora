using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Requests;

public sealed record RejectDocumentRequest(string Reason);
