using Procedure.Application.DTOs;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Queries;

public sealed record GetProcedureProgressQuery(Guid ProfileId)
    : IQuery<Result<ProcedureProgressDto>>;