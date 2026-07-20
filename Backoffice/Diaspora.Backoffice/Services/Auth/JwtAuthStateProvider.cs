using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Diaspora.Backoffice.Services.Auth;

// Forward declaration to resolve circular dependency

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public JwtAuthStateProvider(ILocalStorageService localStorage, IHttpClientFactory httpClientFactory)
    {
        _localStorage = localStorage;
        _httpClient = httpClientFactory.CreateClient();
        //_httpClient = httpClientFactory.CreateClient("GatewayClient");
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("auth_token");
        
        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);
        
        // Vérifier si le token est expiré
        if (IsTokenExpired(token))
        {
            await _localStorage.RemoveItemAsync("auth_token");
            await _localStorage.RemoveItemAsync("refresh_token");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        _currentUser = user;
        return new AuthenticationState(user);
    }

    public async Task MarkUserAsAuthenticated(string token, string refreshToken)
    {
        await _localStorage.SetItemAsync("auth_token", token);
        await _localStorage.SetItemAsync("refresh_token", refreshToken);
        
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);
        _currentUser = user;
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("auth_token");
        await _localStorage.RemoveItemAsync("refresh_token");
        
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        _currentUser = anonymous;
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("auth_token");
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("refresh_token");
    }

    public async Task<bool> RefreshTokenAsync()
    {
        var refreshToken = await GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
        {
            await MarkUserAsLoggedOut();
            return false;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new { RefreshToken = refreshToken });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                if (result != null)
                {
                    await MarkUserAsAuthenticated(result.Token, result.RefreshToken);
                    return true;
                }
            }
        }
        catch
        {
            // Log error
        }

        await MarkUserAsLoggedOut();
        return false;
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Value is System.Text.Json.JsonElement element)
                {
                    if (element.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var item in element.EnumerateArray())
                        {
                            claims.Add(new Claim(kvp.Key, item.ToString()));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(kvp.Key, element.ToString()));
                    }
                }
                else
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? ""));
                }
            }
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    private static bool IsTokenExpired(string jwt)
    {
        try
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            
            if (keyValuePairs?.TryGetValue("exp", out var expValue) == true && expValue is System.Text.Json.JsonElement element)
            {
                var expiration = element.GetInt64();
                var expirationDate = DateTimeOffset.FromUnixTimeSeconds(expiration).UtcDateTime;
                return expirationDate < DateTime.UtcNow;
            }
        }
        catch
        {
            // If we can't parse the token, consider it expired
        }
        
        return true;
    }
}

public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
