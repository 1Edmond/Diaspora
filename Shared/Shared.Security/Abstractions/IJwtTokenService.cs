using System.Security.Claims;
using System.Text;

namespace Shared.Security.Abstractions;

public interface IJwtTokenService
{
    string GenerateAccessToken(
        string userId,
        string email,
        IEnumerable<string> roles,
        string dataLanguage,
        string interfaceLanguage);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    string? GetUserIdFromToken(string token);
}