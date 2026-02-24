using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Commands;

public sealed record CompleteUserProcedureCommand(
    Guid UserProcedureId,
    string CompletedByUserId,
    string CompletedByRole,
    string? Notes
) : ICommand<Result>;