using Shared.Domain.Primitives;
using Shared.Domain.ValueObjects;
using Shared.Security.Abstractions;
using Shared.Domain.Attributes;
using Shared.Security.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Shared.Security.Helpers;

public static class EncryptedValueObjectHelper
{
    public static async Task EncryptEntityAsync<T>(
        T entity,
        IEncryptionService encryptionService,
        IKeyManagementService keyManagement,
        IEncryptionMetadataRepository metadataRepository,
        CancellationToken cancellationToken = default) where T : AggregateRoot
    {
        var entityId = entity.Id.ToString();
        var entityType = typeof(T).Name;

        // 1. Récupérer ou créer la DEK
        var (keyId, dataKey) = await keyManagement.GetOrCreateDataEncryptionKeyAsync(
            entityId,
            entityType,
            cancellationToken);

        try
        {
            var properties = typeof(T)
                .GetProperties()
                .Where(p => p.GetCustomAttribute<SensitiveDataAttribute>() != null)
                .ToList();

            // 2. Paralléliser le chiffrement des propriétés
            var encryptionTasks = properties.Select(async property =>
            {
                var attribute = property.GetCustomAttribute<SensitiveDataAttribute>()!;
                var value = property.GetValue(entity);

                if (value is EncryptedString encryptedString && !encryptedString.IsEncrypted)
                {
                    var encrypted = await encryptionService.EncryptStringAsync(
                        encryptedString.ClearValue,
                        entityId,
                        entityType,
                        cancellationToken);

                    var newValue = encryptedString.WithEncryptedValue(encrypted);

                    // Utiliser reflection pour setter même avec private set
                    property.SetValue(entity, newValue, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);

                    await SaveMetadataAsync(
                        metadataRepository,
                        entityType,
                        entityId,
                        property.Name,
                        keyId, // Utiliser keyId du tuple
                        attribute,
                        encryptedString.ClearValue.Length,
                        cancellationToken);
                }
                else if (value is EncryptedBytes encryptedBytes && !encryptedBytes.IsEncrypted)
                {
                    var encrypted = await encryptionService.EncryptBytesAsync(
                        encryptedBytes.ClearValue,
                        entityId,
                        entityType,
                        cancellationToken);

                    var newValue = encryptedBytes.WithEncryptedValue(encrypted);

                    property.SetValue(entity, newValue, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);

                    await SaveMetadataAsync(
                        metadataRepository,
                        entityType,
                        entityId,
                        property.Name,
                        keyId, // Utiliser keyId du tuple
                        attribute,
                        encryptedBytes.ClearValue.Length,
                        cancellationToken);
                }
            });

            // 3. Attendre que toutes les propriétés soient chiffrées
            await Task.WhenAll(encryptionTasks);
        }
        finally
        {
            // 4. Nettoyer la clé de la mémoire pour la sécurité
            if (dataKey != null) { Array.Clear(dataKey, 0, dataKey.Length); }
        }
    }

    public static async Task DecryptEntityAsync<T>(
        T entity,
        IEncryptionService encryptionService,
        CancellationToken cancellationToken = default) where T : AggregateRoot
    {
        var entityId = entity.Id.ToString();
        var entityType = typeof(T).Name;

        var properties = typeof(T)
            .GetProperties()
            .Where(p => p.GetCustomAttribute<SensitiveDataAttribute>() != null);

        foreach (var property in properties)
        {
            var value = property.GetValue(entity);

            if (value is EncryptedString encryptedString && encryptedString.IsEncrypted)
            {
                var decrypted = await encryptionService.DecryptStringAsync(
                    encryptedString.EncryptedValue!,
                    entityId,
                    entityType,
                    cancellationToken);

                var newValue = EncryptedString.FromEncrypted(encryptedString.EncryptedValue!, decrypted);
                property.SetValue(entity, newValue);
            }
            else if (value is EncryptedBytes encryptedBytes && encryptedBytes.IsEncrypted)
            {
                var decrypted = await encryptionService.DecryptBytesAsync(
                    encryptedBytes.EncryptedValue!,
                    entityId,
                    entityType,
                    cancellationToken);

                var newValue = EncryptedBytes.FromEncrypted(encryptedBytes.EncryptedValue!, decrypted);
                property.SetValue(entity, newValue);
            }
        }
    }

    private static async Task SaveMetadataAsync(
        IEncryptionMetadataRepository repository,
        string entityType,
        string entityId,
        string propertyName,
        Guid keyId,
        SensitiveDataAttribute attribute,
        int dataSize,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetByPropertyAsync(
            entityType,
            entityId,
            propertyName,
            cancellationToken);

        if (existing == null)
        {
            var metadata = new EncryptionMetadataRecord
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                PropertyName = propertyName,
                KeyVersion = "v1",
                KeyId = keyId,
                Algorithm = "AES-256-GCM",
                DataType = attribute.DataType.ToString(),
                EncryptedAt = DateTime.UtcNow,
                DataSizeBytes = dataSize
            };

            await repository.SaveAsync(metadata, cancellationToken);
        }
    }

    public static bool HasSensitiveData<T>()
    {
        return typeof(T)
            .GetProperties()
            .Any(p => p.GetCustomAttribute<SensitiveDataAttribute>() != null);
    }
}