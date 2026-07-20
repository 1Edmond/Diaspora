using System.Net.Http.Json;
using IdentityService.Application.Dtos;
using IdentityService.Application.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Diaspora.Backoffice.Services.Auth;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly JwtAuthStateProvider _authStateProvider;

    public AuthService(IHttpClientFactory httpClientFactory, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClientFactory.CreateClient("GatewayClient");
        _authStateProvider = (JwtAuthStateProvider)authStateProvider;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            var request = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.AccessToken != null)
                {
                    await _authStateProvider.MarkUserAsAuthenticated(result.AccessToken, result.RefreshToken);
                    return new AuthResult { Success = true, Message = "Login successful" };
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new AuthResult { Success = false, Message = errorContent ?? "Login failed" };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Message = $"An error occurred: {ex.Message}" };
        }
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName, string phoneNumber)
    {
        try
        {
            var request = new
            {
                Email = email,
                Password = password,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);

            if (response.IsSuccessStatusCode)
            {
                return new AuthResult { Success = true, Message = "Registration successful" };
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new AuthResult { Success = false, Message = errorContent ?? "Registration failed" };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Message = $"An error occurred: {ex.Message}" };
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            // Optionally call the logout endpoint
            await _httpClient.PostAsync("api/auth/logout", null);
        }
        catch
        {
            // Ignore errors during logout
        }
        finally
        {
            await _authStateProvider.MarkUserAsLoggedOut();
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.IsAuthenticated ?? false;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _authStateProvider.GetTokenAsync();
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/auth/me");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDto>();
            }
        }
        catch
        {
            // Handle error
        }
        return null;
    }

    public async Task<bool> HasPermissionAsync(string permission)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.HasClaim("Permission", permission) || 
               authState.User.IsInRole("Admin");
    }
}

public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
