namespace Diaspora.Backoffice.Services.Auth;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly JwtAuthStateProvider _authStateProvider;

    public AuthorizationMessageHandler(JwtAuthStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authStateProvider.GetTokenAsync();
        
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // If unauthorized, try to refresh token
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await _authStateProvider.RefreshTokenAsync();
            if (refreshed)
            {
                // Retry with new token
                token = await _authStateProvider.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }
        }

        return response;
    }
}
