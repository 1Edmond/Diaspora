using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application.Interfaces;

public interface IFileStorageService
{
    Task<Result<string>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
    Task<Result<string>> UploadEncryptedFileAsync(
       Stream fileStream,
       string fileName,
       string contentType,
       string entityId,
       string entityType,
       CancellationToken cancellationToken = default);

    Task<Result<Stream>> DownloadFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);

    Task<Result<Stream>> DownloadDecryptedFileAsync(
        string fileUrl,
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);
}