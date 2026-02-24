using Procedure.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Interfaces;

public interface ILocationRepository : IRepository<Location>
{
    Task<IEnumerable<Location>> GetAllActiveAsync(
        CancellationToken cancellationToken = default);
}