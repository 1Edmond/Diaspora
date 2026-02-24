using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Security.Abstractions;
using Shared.Security.Models;
using System.Security.Cryptography;

public class VaultKeyManagementService : IKeyManagementService
{
    private readonly IDataEncryptionKeyRepository _dekRepository;
    private readonly IVaultTransitService _vaultTransitService;
    private readonly ILogger<VaultKeyManagementService> _logger;
    private readonly string _masterKeyName;

    public VaultKeyManagementService(
        IDataEncryptionKeyRepository dekRepository,
        IVaultTransitService vaultTransitService,
        IConfiguration configuration,
        ILogger<VaultKeyManagementService> logger)
    {
        _dekRepository = dekRepository;
        _vaultTransitService = vaultTransitService;
        _logger = logger;
        _masterKeyName = configuration["Vault:KeyName"]
            ?? throw new ArgumentNullException(nameof(configuration), "Vault:KeyName is required");
    }

    public async Task<(Guid KeyId, byte[] DataKey)> GetOrCreateDataEncryptionKeyAsync(
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        var existingKey = await _dekRepository.GetActiveAsync(
            entityId,
            entityType,
            cancellationToken);

        if (existingKey != null)
        {
            var decryptedKey = await _vaultTransitService.DecryptAsync(
                existingKey.EncryptedKey,
                _masterKeyName,
                cancellationToken);

            return (existingKey.Id, decryptedKey);
        }

        return await CreateNewDataEncryptionKeyAsync(
            entityId,
            entityType,
            cancellationToken);
    }

    public async Task<byte[]?> GetDataEncryptionKeyAsync(
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        var existingKey = await _dekRepository.GetActiveAsync(
            entityId,
            entityType,
            cancellationToken);

        if (existingKey == null)
        {
            return null;
        }

        var decryptedKey = await _vaultTransitService.DecryptAsync(
            existingKey.EncryptedKey,
            _masterKeyName,
            cancellationToken);

        return decryptedKey;
    }

    public async Task<Guid?> GetActiveKeyIdAsync(
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        var activeKey = await _dekRepository.GetActiveAsync(
            entityId,
            entityType,
            cancellationToken);

        return activeKey?.Id;
    }

    private async Task<(Guid KeyId, byte[] DataKey)> CreateNewDataEncryptionKeyAsync(
        string entityId,
        string entityType,
        CancellationToken cancellationToken)
    {
        // 1. Générer une nouvelle DEK
        var plainDek = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(plainDek);
        }

        // 2. Chiffrer la DEK avec Vault
        var encryptedDek = await _vaultTransitService.EncryptAsync(
            plainDek,
            _masterKeyName,
            cancellationToken);

        // 3. Sauvegarder la DEK chiffrée
        var keyId = Guid.NewGuid();
        var dekRecord = new DataEncryptionKey
        {
            Id = keyId,
            EntityId = entityId,
            EntityType = entityType,
            EncryptedKey = encryptedDek,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _dekRepository.SaveAsync(dekRecord, cancellationToken);

        _logger.LogInformation(
            "Created new DEK for entity {EntityType}/{EntityId} with key ID {KeyId}",
            entityType, entityId, keyId);

        // 4. Retourner le tuple (KeyId, DataKey)
        return (keyId, plainDek);
    }
}