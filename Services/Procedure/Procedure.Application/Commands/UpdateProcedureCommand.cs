using Procedure.Application.Requests;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Commands;

public sealed record UpdateProcedureCommand(
    Guid ProcedureId,
    string Title,
    string Description,
    decimal CostAmount,
    string CostCurrency,
    int EstimatedDurationDays,
    List<Guid> LocationIds,
    List<InlineLocationRequest> InlineLocations,
    List<Guid> DependencyIds,
    List<Guid> RequiredDocumentTypeIds
) : ICommand<Result>;