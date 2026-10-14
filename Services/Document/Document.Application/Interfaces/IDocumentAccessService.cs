using System;
using System.Threading;
using System.Threading.Tasks;

namespace Document.Application.Interfaces;

public interface IDocumentAccessService
{
    Task<Guid?> GetCurrentProfileIdAsync(CancellationToken cancellationToken = default);
    Task<bool> CanAccessProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessDocumentAsync(Domain.Entities.Document document, CancellationToken cancellationToken = default);
    bool IsAdminOrOfficer();
}
