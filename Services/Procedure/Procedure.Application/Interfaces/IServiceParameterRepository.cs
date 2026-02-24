using Procedure.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Interfaces;

public interface IServiceParameterRepository : IRepository<ServiceParameter>
{
    Task<ServiceParameter?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<bool> GetBooleanAsync(
        string key,
        bool defaultValue = false,
        CancellationToken cancellationToken = default);
}
