using Microsoft.Extensions.Configuration;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Security.Services;

public class FileVaultTokenProvider : IVaultTokenProvider
{
    private readonly string _tokenPath;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public FileVaultTokenProvider(IConfiguration configuration)
    {
        _tokenPath = configuration["Vault:TokenPath"]
            ?? throw new ArgumentNullException("Vault:TokenPath not configured");
    }

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry)
        {
            return _cachedToken;
        }

        if (!File.Exists(_tokenPath))
        {
            throw new InvalidOperationException($"Vault token file not found at {_tokenPath}");
        }

        _cachedToken = (await File.ReadAllTextAsync(_tokenPath, ct)).Trim();
        _tokenExpiry = DateTime.UtcNow.AddMinutes(25);

        return _cachedToken;
    }
}
