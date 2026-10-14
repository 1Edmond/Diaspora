using Document.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Interfaces;

public interface IDocumentTypeRepository : IRepository<DocumentType>
{
    Task<DocumentType?> GetByCodeAsync(
        string code, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(
        string code, CancellationToken cancellationToken = default);

    Task<IEnumerable<DocumentType>> GetAllActiveAsync(
        CancellationToken cancellationToken = default);

    Task<IEnumerable<DocumentType>> GetActiveByProfileTypeAsync(
        Guid? profileTypeId, CancellationToken cancellationToken = default);
}
