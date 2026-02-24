using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Domain.Entities;

public sealed class ProcedureRequiredDocument : Entity
{
    public Guid ProcedureId { get; private set; }
    public Guid DocumentTypeId { get; private set; }

    private ProcedureRequiredDocument() { }

    public ProcedureRequiredDocument(Guid procedureId, Guid documentTypeId)
        : base(Guid.NewGuid())
    {
        ProcedureId = procedureId;
        DocumentTypeId = documentTypeId;
    }
}