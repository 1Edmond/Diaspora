using IdentityService.Domain.Entities;
using Shared.Domain.Interfaces;

namespace IdentityService.Application.Abstractions;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}
