using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.DTOs;

public sealed record UserProcedureDto(
    Guid Id,
    Guid ProfileId,
    Guid ProcedureId,
    string ProcedureTitle,
    decimal? CostAmount,
    string? CostCurrency,
    UserProcedureStatus Status,
    string? CompletedByUserId,
    string? CompletedByRole,
    DateTime? CompletedAt,
    string? Notes,
    DateTime CreatedAt
);