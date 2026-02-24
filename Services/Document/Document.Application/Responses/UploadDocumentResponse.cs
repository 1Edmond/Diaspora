using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Responses;

public sealed record UploadDocumentResponse(Guid DocumentId, string FileName);
