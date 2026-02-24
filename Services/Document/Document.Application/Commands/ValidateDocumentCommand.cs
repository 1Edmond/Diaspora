using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Commands;

public sealed record ValidateDocumentCommand(
    Guid DocumentId,
    string AdminId
) : ICommand<Result>;