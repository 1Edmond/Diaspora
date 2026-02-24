using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Interfaces;

public interface IProcedureRepository : IRepository<Domain.Entities.Procedure>
{
    Task<IEnumerable<Domain.Entities.Procedure>> GetByProfileTypeAsync(
        ProcedureProfileType profileType,
        Guid profileTypeId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Domain.Entities.Procedure>> GetWithDependenciesAsync(
        Guid procedureId,
        CancellationToken cancellationToken = default);
}
