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
    Guid? LocationId,
    List<Guid> DependencyIds,
    List<Guid> RequiredDocumentTypeIds
) : ICommand<Result>;