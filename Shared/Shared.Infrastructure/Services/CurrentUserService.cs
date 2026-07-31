using Microsoft.AspNetCore.Http;
using Shared.Application.Interfaces;
using Shared.Security.Constants;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Shared.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName => User?.Identity?.Name;

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public string DataLanguage => User?.FindFirstValue(JwtClaimTypes.DataLanguage) ?? "fr";

    public string InterfaceLanguage => User?.FindFirstValue(JwtClaimTypes.InterfaceLanguage) ?? "fr";

    public IEnumerable<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(r => r.Value) ?? Enumerable.Empty<string>();

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => Roles.Contains(role);
}