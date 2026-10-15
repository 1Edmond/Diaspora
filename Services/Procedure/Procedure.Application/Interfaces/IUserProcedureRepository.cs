using Procedure.Domain.Entities;
using Shared.Application.Models;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Interfaces;

public interface IUserProcedureRepository : IRepository<UserProcedure>
{
    Task<IEnumerable<UserProcedure>> GetByProfileIdAsync(
        Guid profileId,
        CancellationToken cancellationToken = default);

    Task<UserProcedure?> GetByProfileAndProcedureAsync(
        Guid profileId,
        Guid procedureId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<UserProcedure>> GetDependencyStatusesAsync(
        Guid profileId,
        IEnumerable<Guid> dependencyProcedureIds,
        CancellationToken cancellationToken = default);

    Task<PagedList<UserProcedure>> GetAllAdminAsync(
        UserProcedureStatus? status = null,
        Guid? procedureId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}