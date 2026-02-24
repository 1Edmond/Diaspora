using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Infrastructure.Services;

public sealed class EncryptedLocalFileStorageService(
    IEncryptionService encryptionService,
    IConfiguration configuration,
    ILogger<EncryptedLocalFileStorageService> logger) : IFileStorageService
{
    private readonly string _basePath = configuration["FileStorage:BasePath"]
        ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

    public async Task<Result<string>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var folder = Path.Combine(_basePath, "plain");
            Directory.CreateDirectory(folder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var filePath = Path.Combine(folder, uniqueFileName);

            await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fs, cancellationToken);

            logger.LogInformation("File saved (plain): {FilePath}", filePath);

            return Result.Success(filePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save file: {FileName}", fileName);
            return Result.Failure<string>($"Failed to save file: {ex.Message}");
        }
    }

    
    public async Task<Result<string>> UploadEncryptedFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Lire le stream
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream, cancellationToken);
            var plainBytes = memoryStream.ToArray();

            // 2. Chiffrer avec la DEK du profil (AES-GCM — nonce + tag + ciphertext)
            var encryptedBytes = await encryptionService.EncryptBytesAsync(
                plainBytes, entityId, entityType, cancellationToken);

            // 3. Écrire sur le volume — dossier par entityId pour retrouver la DEK au download
            var folder = Path.Combine(_basePath, entityId);
            Directory.CreateDirectory(folder);

            // Extension .enc pour marquer que le fichier est chiffré
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}.enc";
            var filePath = Path.Combine(folder, uniqueFileName);

            await File.WriteAllBytesAsync(filePath, encryptedBytes, cancellationToken);

            logger.LogInformation(
                "File encrypted and saved: {FilePath} ({Size} bytes)",
                filePath, encryptedBytes.Length);

            return Result.Success(filePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to encrypt and save file: {FileName}", fileName);
            return Result.Failure<string>($"Failed to save encrypted file: {ex.Message}");
        }
    }

    // ── Download en clair ─────────────────────────────────────────────────────

    public async Task<Result<Stream>> DownloadFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(fileUrl))
                return Result.Failure<Stream>($"File not found: {fileUrl}");

            var bytes = await File.ReadAllBytesAsync(fileUrl, cancellationToken);
            return Result.Success<Stream>(new MemoryStream(bytes));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download file: {FileUrl}", fileUrl);
            return Result.Failure<Stream>($"Failed to download file: {ex.Message}");
        }
    }

    // ── Download déchiffré ────────────────────────────────────────────────────

    public async Task<Result<Stream>> DownloadDecryptedFileAsync(
        string fileUrl,
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(fileUrl))
                return Result.Failure<Stream>($"File not found: {fileUrl}");

            // 1. Lire les bytes chiffrés
            var encryptedBytes = await File.ReadAllBytesAsync(fileUrl, cancellationToken);

            // 2. Déchiffrer avec la DEK (entityId + entityType fournis par l'appelant)
            var plainBytes = await encryptionService.DecryptBytesAsync(
                encryptedBytes, entityId, entityType, cancellationToken);

            logger.LogInformation(
                "File decrypted and served: {FileUrl} ({Size} bytes)",
                fileUrl, plainBytes.Length);

            return Result.Success<Stream>(new MemoryStream(plainBytes));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to decrypt file: {FileUrl}", fileUrl);
            return Result.Failure<Stream>($"Failed to decrypt file: {ex.Message}");
        }
    }

    // ── Suppression ───────────────────────────────────────────────────────────

    public async Task<Result> DeleteFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(fileUrl))
            {
                logger.LogWarning("File not found for deletion: {FileUrl}", fileUrl);
                return Result.Success();
            }

            await Task.Run(() => File.Delete(fileUrl), cancellationToken);

            logger.LogInformation("File deleted: {FileUrl}", fileUrl);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete file: {FileUrl}", fileUrl);
            return Result.Failure($"Failed to delete file: {ex.Message}");
        }
    }
}
