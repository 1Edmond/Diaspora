using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Domain.Entities;

public sealed class ProcedureDependency : Entity
{
    public Guid ProcedureId { get; private set; }
    public Guid DependsOnProcedureId { get; private set; }

    public Procedure DependsOnProcedure { get; private set; } = null!;

    private ProcedureDependency() { }

    public ProcedureDependency(Guid procedureId, Guid dependsOnProcedureId)
        : base(Guid.NewGuid())
    {
        ProcedureId = procedureId;
        DependsOnProcedureId = dependsOnProcedureId;
    }
}
