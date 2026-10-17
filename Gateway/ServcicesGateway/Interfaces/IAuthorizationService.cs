using ServcicesGateway.Models;

namespace ServcicesGateway.Interfaces;

public interface IAuthorizationService
{
    Task<AuthorizationCheckResponse?> GetPermissionAsync(
     Guid userId,
     CancellationToken cancellationToken = default);
    Task<AuthorizationCheckResponse?> CheckPermissionAsync(Guid userId, string path, string method, CancellationToken cancellationToken = default);
    Task InvalidateCacheAsync(Guid userId, string? path = null, string? method = null, CancellationToken cancellationToken = default);
}