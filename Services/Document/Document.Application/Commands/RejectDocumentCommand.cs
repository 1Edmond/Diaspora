using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Commands;

public sealed record RejectDocumentCommand(
    Guid DocumentId,
    string AdminId,
    string Reason
) : ICommand<Result>;
