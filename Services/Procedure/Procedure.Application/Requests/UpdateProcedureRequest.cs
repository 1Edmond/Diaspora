using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Requests;

public sealed record UpdateProcedureRequest(
    string Title,
    string Description,
    decimal CostAmount,
    string CostCurrency,
    int EstimatedDurationDays,
    Guid? LocationId,
    List<Guid> DependencyIds,
    List<Guid> RequiredDocumentTypeIds
);
