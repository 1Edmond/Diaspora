using Procedure.Domain.Enums;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Domain.Entities;

public sealed class ProcedureDocument : Entity
{
    public Guid ProcedureId { get; private set; }
    public Guid DocumentTypeId { get; private set; }

    public DocumentRole Role { get; private set; }

    private ProcedureDocument() { }

    public ProcedureDocument(Guid procedureId, Guid documentTypeId, DocumentRole role)
        : base(Guid.NewGuid())
    {
        ProcedureId = procedureId;
        DocumentTypeId = documentTypeId;
        Role = role;
    }
}