using Document.Application.DTOs;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Queries;

public sealed record GetDocumentByIdQuery(Guid DocumentId)
    : IQuery<Result<DocumentDto>>;
