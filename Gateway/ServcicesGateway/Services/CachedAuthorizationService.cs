using Microsoft.Extensions.Caching.Distributed;
using ServcicesGateway.Interfaces;
using ServcicesGateway.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServcicesGateway.Services;

public class CachedAuthorizationService : IAuthorizationService
{
    private readonly HttpClient _httpClient;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedAuthorizationService> _logger;
    private readonly TimeSpan _cacheExpiration;

    public CachedAuthorizationService(
        IHttpClientFactory httpClientFactory,
        IDistributedCache cache,
        ILogger<CachedAuthorizationService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("IdentityService");
        _cache = cache;
        _logger = logger;

        var expirationMinutes = configuration.GetValue<int>("AuthorizationCache:DefaultExpirationMinutes", 5);
        _cacheExpiration = TimeSpan.FromMinutes(expirationMinutes);
    }

    public async Task<AuthorizationCheckResponse?> CheckPermissionAsync(
     Guid userId,
     string path,
     string method,
     CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(userId, path, method);
        var cachedValue = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedValue))
        {
            var cachedResult = JsonSerializer.Deserialize<AuthorizationCheckResponse>(cachedValue);
             if(_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("[Gateway] 🟢 Cache HIT: User={UserId}, {Method} {Path}", userId, method, path);
            return cachedResult!;
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/authorization/check?userId={userId}&path={Uri.EscapeDataString(path)}&method={method}",
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AuthorizationCheckResponse>(cancellationToken);
           
            await _cache.SetStringAsync(
                cacheKey,
                result!.ToJson(),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheExpiration
                },
                cancellationToken
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Gateway] ❌ Error checking permission");
            return null;
        }
    }
    

    public async Task<AuthorizationCheckResponse?> GetPermissionAsync(
     Guid userId,
     CancellationToken cancellationToken = default)
    {
        var cacheKey = GetUserPermissionCacheKey(userId);
        var cachedValue = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedValue))
        {
            var cachedResult = JsonSerializer.Deserialize<AuthorizationCheckResponse>(cachedValue);
             if(_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("[Gateway] 🟢 Cache HIT: User={UserId}", userId);
            return cachedResult!;
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/authorization/user-permissions?userId={userId}",
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AuthorizationCheckResponse>(cancellationToken);
           
            await _cache.SetStringAsync(
                cacheKey,
                result!.ToJson(),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheExpiration
                },
                cancellationToken
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Gateway] ❌ Error checking permission");
            return null;
        }
    }
    
    
    public async Task InvalidateCacheAsync(Guid userId, string? path = null, string? method = null)
    {
        if (string.IsNullOrEmpty(path))
        {
           if(_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation(
                "[Gateway] 🔄 Cache invalidation requested for User={UserId} (all permissions)",
                userId
            );
            var userKeysSetKey = $"perm:keys:{userId}";
            var userKeysValue = await _cache.GetStringAsync(userKeysSetKey);

            if (!string.IsNullOrEmpty(userKeysValue))
            {
                var keys = userKeysValue.Split(';', StringSplitOptions.RemoveEmptyEntries);

                foreach (var key in keys)
                {
                    await _cache.RemoveAsync(key);
                    _logger.LogDebug("[Gateway] 🗑️ Removed cache key: {Key}", key);
                }
                await _cache.RemoveAsync(userKeysSetKey);
            }

            _logger.LogInformation(
                "[Gateway] ✅ All cache invalidated for User={UserId}",
                userId
            );
        }
        else
        {
            if (string.IsNullOrEmpty(method))
            {
                var methods = new[] { "GET", "POST", "PUT", "PATCH", "DELETE" };

                foreach (var m in methods)
                {
                    var cacheKey = GetCacheKey(userId, path, m);
                    await _cache.RemoveAsync(cacheKey);
                    _logger.LogDebug(
                        "[Gateway] 🗑️ Removed cache key: User={UserId}, {Method} {Path}",
                        userId, m, path
                    );
                }
            }
            else
            {
                var cacheKey = GetCacheKey(userId, path, method);
                await _cache.RemoveAsync(cacheKey);

                _logger.LogInformation(
                    "[Gateway] 🔄 Cache invalidated: User={UserId}, {Method} {Path}",
                    userId, method, path
                );
            }
        }
    }

    private static string GetCacheKey(Guid userId, string path, string method)
     => $"perm:{userId}:{method}:{path}";
    private static string GetUserPermissionCacheKey(Guid userId)
     => $"perm:{userId}";
}