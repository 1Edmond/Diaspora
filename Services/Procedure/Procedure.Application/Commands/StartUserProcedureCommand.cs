using Procedure.Application.Responses;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Commands;

public sealed record StartUserProcedureCommand(
    Guid ProfileId,
    Guid ProcedureId
) : ICommand<Result<StartUserProcedureResponse>>;