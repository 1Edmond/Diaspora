using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IConfiguration configuration, IEncryptionService encryptionService, ILogger<LocalFileStorageService> logger)
    {
        _basePath = configuration["FileStorage:BasePath"] ?? "uploads";
        _logger = logger;
        _encryptionService = encryptionService;

        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    private bool IsValidPath(string targetPath, out string fullPath)
    {
        var baseFullPath = Path.GetFullPath(_basePath);
        fullPath = Path.GetFullPath(targetPath);
        return fullPath.StartsWith(baseFullPath, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<Result<string>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (fileStream == null || fileStream.Length == 0)
                return Result.Failure<string>("File stream is empty");

            var filePath = Path.Combine(_basePath, fileName);
            if (!IsValidPath(filePath, out var fullPath))
                return Result.Failure<string>("Invalid file path");

            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var fileStreamOutput = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);
            }

            return Result.Success(filePath);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("File upload was cancelled");
            return Result.Failure<string>("File upload was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            return Result.Failure<string>("Error uploading file");
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
            if (fileStream == null || fileStream.Length == 0)
                return Result.Failure<string>("File stream is empty");

            var filePath = Path.Combine(_basePath, fileName);
            if (!IsValidPath(filePath, out var fullPath))
                return Result.Failure<string>("Invalid file path");

            _logger.LogInformation(
                "Encrypting and uploading file: {FileName} for {EntityType}:{EntityId}",
                fileName,
                entityType,
                entityId);

            // Read file bytes
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream, cancellationToken);
            var fileBytes = memoryStream.ToArray();

            // Encrypt file content using HashiCorp Vault
            var encryptedBytes = await _encryptionService.EncryptBytesAsync(
                fileBytes,
                entityId,
                entityType,
                cancellationToken);

            // Save encrypted file
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllBytesAsync(fullPath, encryptedBytes, cancellationToken);

            _logger.LogInformation(
                "Successfully encrypted and saved file at: {FilePath}",
                filePath);

            return Result.Success(filePath);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("File upload was cancelled");
            return Result.Failure<string>("File upload was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting and uploading file: {FileName}", fileName);
            return Result.Failure<string>("Error encrypting and uploading file");
        }
    }

    public async Task<Result<Stream>> DownloadFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsValidPath(fileUrl, out var fullPath))
                return Result.Failure<Stream>("Invalid file path");

            if (!File.Exists(fullPath))
                return Result.Failure<Stream>("File not found");

            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(memoryStream, cancellationToken);
            }

            memoryStream.Position = 0;
            return Result.Success<Stream>(memoryStream);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("File download was cancelled");
            return Result.Failure<Stream>("File download was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FileUrl}", fileUrl);
            return Result.Failure<Stream>("Error downloading file");
        }
    }

    public async Task<Result<Stream>> DownloadDecryptedFileAsync(
        string fileUrl,
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsValidPath(fileUrl, out var fullPath))
                return Result.Failure<Stream>("Invalid file path");

            if (!File.Exists(fullPath))
                return Result.Failure<Stream>("File not found");

            _logger.LogInformation(
                "Downloading and decrypting file: {FileUrl} for {EntityType}:{EntityId}",
                fileUrl,
                entityType,
                entityId);

            // Read encrypted file
            var encryptedBytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);

            // Decrypt using HashiCorp Vault
            var decryptedBytes = await _encryptionService.DecryptBytesAsync(
                encryptedBytes,
                entityId,
                entityType,
                cancellationToken);

            var memoryStream = new MemoryStream(decryptedBytes);
            memoryStream.Position = 0;

            _logger.LogInformation("Successfully decrypted file: {FileUrl}", fileUrl);

            return Result.Success<Stream>(memoryStream);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("File download was cancelled");
            return Result.Failure<Stream>("File download was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading and decrypting file: {FileUrl}", fileUrl);
            return Result.Failure<Stream>("Error downloading and decrypting file");
        }
    }

    public async Task<Result> DeleteFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsValidPath(fileUrl, out var fullPath))
                return Result.Failure("Invalid file path");

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath), cancellationToken);
            }

            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("File deletion was cancelled");
            return Result.Failure("File deletion was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
            return Result.Failure("Error deleting file");
        }
    }
}
