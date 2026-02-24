using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Commands;

public sealed record DeleteDocumentCommand(
    Guid DocumentId,
    Guid RequestingUserId
) : ICommand<Result>;