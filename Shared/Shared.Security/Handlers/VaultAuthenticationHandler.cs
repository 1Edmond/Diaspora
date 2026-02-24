using Microsoft.Extensions.Logging;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Security.Handlers;

public class VaultAuthenticationHandler : DelegatingHandler
{
    private readonly IVaultTokenProvider _tokenProvider;
    private readonly ILogger<VaultAuthenticationHandler> _logger;

    public VaultAuthenticationHandler(
        IVaultTokenProvider tokenProvider,
        ILogger<VaultAuthenticationHandler> logger)
    {
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        request.Headers.Add("X-Vault-Token", token);

        _logger.LogDebug("Adding Vault token to request: {Uri}", request.RequestUri);

        return await base.SendAsync(request, cancellationToken);
    }
}