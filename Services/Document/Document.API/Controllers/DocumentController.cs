using Document.Application.Commands;
using Document.Application.DTOs;
using Document.Application.Handlers.Commands;
using Document.Application.Queries;
using Document.Application.Requests;
using Document.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Document.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController(
    IMessageBus _messageBus,
    IFileStorageService _fileStorageService,
    ICurrentUserService _currentUserService,
    ILogger<DocumentController> _logger) : ControllerBase
{
    [Permission(
        resourceKey: "document",
        action: "upload",
        description: "Upload a document for an external profile",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromForm] UploadDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UploadDocumentCommand(
            request.ProfileId,
            request.DocumentTypeId,
            request.File,
            request.ExpiresAt,
            request.IssuedAt,
            request.IssuedBy);

        var result = await _messageBus.InvokeAsync<Result<UploadDocumentResponse>>(
            command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "document",
        action: "read",
        description: "Get a document by ID",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("{documentId:guid}")]
    public async Task<IActionResult> GetById(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<DocumentDto>>(
            new GetDocumentByIdQuery(documentId), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "document",
        action: "read-by-profile",
        description: "Get all documents of an external profile",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("profile/{profileId:guid}")]
    public async Task<IActionResult> GetByProfile(
        Guid profileId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _messageBus.InvokeAsync<Result<PagedList<DocumentDto>>>(
            new GetDocumentsByProfileQuery(profileId, pageNumber, pageSize),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "document",
        action: "read-expiring",
        description: "Get documents expiring soon (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiring(
        [FromQuery] int daysAhead = 30,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _messageBus.InvokeAsync<Result<PagedList<DocumentDto>>>(
            new GetExpiringDocumentsQuery(daysAhead, pageNumber, pageSize),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "document",
        action: "validate",
        description: "Validate a pending document (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{documentId:guid}/validate")]
    public async Task<IActionResult> Validate(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new ValidateDocumentCommand(documentId, _currentUserService.UserId!),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Document validated successfully" });
    }

    [Permission(
        resourceKey: "document",
        action: "reject",
        description: "Reject a pending document (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{documentId:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid documentId,
        [FromBody] RejectDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new RejectDocumentCommand(documentId, _currentUserService.UserId!, request.Reason),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Document rejected successfully" });
    }

    [Permission(
        resourceKey: "document",
        action: "delete",
        description: "Delete a document",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpDelete("{documentId:guid}")]
    public async Task<IActionResult> Delete(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new DeleteDocumentCommand(documentId, GetCurrentUserId()),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Document deleted successfully" });
    }

    [Permission(
        resourceKey: "document",
        action: "download",
        description: "Download and decrypt a document file",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("{documentId:guid}/download")]
    public async Task<IActionResult> Download(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        // 1. Récupérer les métadonnées du document (contient FilePath et ExternalProfileId)
        var docResult = await _messageBus.InvokeAsync<Result<DocumentDto>>(
            new GetDocumentByIdQuery(documentId), cancellationToken);

        if (docResult.IsFailure)
            return NotFound(new { error = docResult.Error });

        var doc = docResult.Value;

        // 2. Déchiffrer et streamer le fichier
        var streamResult = await _fileStorageService.DownloadDecryptedFileAsync(
            doc.FilePath,
            entityId: doc.ProfileId.ToString(),
            entityType: UploadDocumentCommandHandler.FileEntityType,
            cancellationToken);

        if (streamResult.IsFailure)
            return StatusCode(500, new { error = streamResult.Error });

        return File(streamResult.Value, doc.MimeType, doc.FileName);
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}