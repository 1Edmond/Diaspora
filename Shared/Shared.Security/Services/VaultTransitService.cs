using Microsoft.Extensions.Logging;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Shared.Security.Services;

public class VaultTransitService(HttpClient client, ILogger<VaultTransitService> logger) : IVaultTransitService
{
    private readonly HttpClient _client = client;
    private readonly ILogger<VaultTransitService> _logger = logger;

    public async Task<byte[]> EncryptAsync(byte[] plain, string keyName, CancellationToken ct)
    {
        var response = await _client.PostAsJsonAsync(
            $"v1/transit/encrypt/{keyName}",
            new { plaintext = Convert.ToBase64String(plain) },
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "Vault encryption failed: {StatusCode} - {Error}",
                response.StatusCode,
                errorContent);
            throw new HttpRequestException(
                $"Vault encryption failed: {response.StatusCode} - {errorContent}");
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        var ciphertext = json.GetProperty("data").GetProperty("ciphertext").GetString()!;

        return Encoding.UTF8.GetBytes(ciphertext);
    }

    public async Task<byte[]> DecryptAsync(byte[] cipher, string keyName, CancellationToken ct)
    {
        // Le token est automatiquement ajouté par le VaultAuthenticationHandler

        // Convertir les bytes en string (format: "vault:v1:base64data")
        var ciphertext = Encoding.UTF8.GetString(cipher);

        var response = await _client.PostAsJsonAsync(
            $"v1/transit/decrypt/{keyName}",
            new { ciphertext = ciphertext },
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "Vault decryption failed: {StatusCode} - {Error}",
                response.StatusCode,
                errorContent);
            throw new HttpRequestException(
                $"Vault decryption failed: {response.StatusCode} - {errorContent}");
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        var plaintext = json.GetProperty("data").GetProperty("plaintext").GetString()!;

        return Convert.FromBase64String(plaintext);
    }
}
