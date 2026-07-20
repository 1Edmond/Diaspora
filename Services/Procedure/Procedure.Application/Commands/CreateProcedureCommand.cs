using Procedure.Application.Requests;
using Procedure.Application.Responses;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Commands;


public sealed record CreateProcedureCommand(
    string Title,
    string Description,
    decimal CostAmount,
    string CostCurrency,
    ProcedureProfileType ProfileType,
    Guid ProfileTypeId,
    int EstimatedDurationDays,
    List<Guid> LocationIds,
    List<InlineLocationRequest> InlineLocations,
    List<Guid> DependencyIds,
    List<Guid> RequiredDocumentTypeIds
) : ICommand<Result<CreateProcedureResponse>>;