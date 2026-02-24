using Document.Application.Extensions;
using Document.Application.Interfaces;
using Document.Application.Requests;
using Document.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.Security.Attributes;

namespace Document.API.Controllers;

[ApiController]
[Route("api/document-types")]
public class DocumentTypeController(
    IDocumentTypeRepository _repository,
    ILogger<DocumentTypeController> _logger) : ControllerBase
{
    [Permission(
        resourceKey: "document.type",
        action: "list",
        description: "List all active document types",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var types = await _repository.GetAllActiveAsync(cancellationToken);
        return Ok(types.Select(t => t.ToDto()));
    }

    [Permission(
        resourceKey: "document.type",
        action: "create",
        description: "Create a new document type (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDocumentTypeRequest request,
        CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsByCodeAsync(request.Code, cancellationToken);
        if (exists)
            return Conflict(new { error = $"Document type with code '{request.Code}' already exists." });

        var result = DocumentType.Create(
            request.Name,
            request.Code,
            request.Description,
            request.IsRenewable,
            request.RenewalAlertDays,
            request.IsRequired);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        await _repository.AddAsync(result.Value, cancellationToken);

        return Ok(result.Value.ToDto());
    }

    [Permission(
        resourceKey: "document.type",
        action: "deactivate",
        description: "Deactivate a document type (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var docType = await _repository.GetByIdAsync(id, cancellationToken);
        if (docType is null)
            return NotFound(new { error = "Document type not found." });

        docType.Deactivate();
        await _repository.UpdateAsync(docType, cancellationToken);

        return Ok(new { message = "Document type deactivated." });
    }
}