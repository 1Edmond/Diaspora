using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Domain.Entities;

public sealed class ProcedureLocation : Entity
{
    public Guid ProcedureId { get; private set; }
    public Guid LocationId { get; private set; }

    public Location Location { get; private set; } = null!;

    private ProcedureLocation() { }

    public ProcedureLocation(Guid procedureId, Guid locationId) : base(Guid.NewGuid())
    {
        ProcedureId = procedureId;
        LocationId = locationId;
    }
}