using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.DTOs;

public sealed record ProcedureDto(
    Guid Id,
    string Title,
    string Description,
    decimal CostAmount,
    string CostCurrency,
    ProcedureProfileType ProfileType,
    Guid ProfileTypeId,
    int EstimatedDurationDays,
    bool IsActive,
    Guid? LocationId,
    LocationDto? Location,
    IReadOnlyList<Guid> DependencyIds,
    IReadOnlyList<Guid> RequiredDocumentTypeIds,
    DateTime CreatedAt
);