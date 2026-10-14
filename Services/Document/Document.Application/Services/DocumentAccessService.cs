using Document.Application.Interfaces;
using Shared.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Document.Application.Services;

public sealed class DocumentAccessService(
    IProfileDirectoryRepository _profileDirectoryRepository,
    ICurrentUserService _currentUserService) : IDocumentAccessService
{
    public bool IsAdminOrOfficer()
    {
        return _currentUserService.IsInRole("Admin")
            || _currentUserService.IsInRole("Administrator")
            || _currentUserService.IsInRole("Officer");
    }

    public async Task<Guid?> GetCurrentProfileIdAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_currentUserService.UserId) ||
            !Guid.TryParse(_currentUserService.UserId, out var userId))
        {
            return null;
        }

        var entry = await _profileDirectoryRepository.GetByUserIdAsync(userId, cancellationToken);
        return entry?.Id;
    }

    public async Task<bool> CanAccessProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        if (IsAdminOrOfficer())
            return true;

        if (profileId == Guid.Empty)
            return false;

        var currentProfileId = await GetCurrentProfileIdAsync(cancellationToken);
        return currentProfileId.HasValue && currentProfileId.Value == profileId;
    }

    public async Task<bool> CanAccessDocumentAsync(Domain.Entities.Document document, CancellationToken cancellationToken = default)
    {
        if (document is null)
            return false;

        return await CanAccessProfileAsync(document.ProfileId, cancellationToken);
    }
}
