using Document.Application.DTOs;
using Document.Application.Extensions;
using Document.Application.Interfaces;
using Document.Application.Requests;
using Document.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Interfaces;
using Shared.Security.Attributes;

namespace Document.API.Controllers;

[ApiController]
[Route("api/document-types")]
public class DocumentTypeController(
    IDocumentTypeRepository _repository,
    Shared.Application.Abstractions.ITranslationRepository<DocumentType> _translationRepo,
    Shared.Application.Interfaces.ICurrentUserService _currentUserService,
    IUnitOfWork _unitOfWork,
    ILogger<DocumentTypeController> _logger) : ControllerBase
{
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [HttpGet("public")]
    public async Task<IActionResult> GetPublic(
        [FromQuery] Guid? profileTypeId,
        CancellationToken cancellationToken)
    {
        return await GetAll(profileTypeId, includeInactive: false, cancellationToken);
    }

    [Permission(
        resourceKey: "document.type",
        action: "list",
        description: "List all document types",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? profileTypeId,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        List<DocumentType> types;
        if (includeInactive)
        {
            var all = await _repository.GetAllAsync(cancellationToken);
            if (profileTypeId.HasValue && profileTypeId.Value != Guid.Empty)
            {
                all = all.Where(x => x.ProfileTypeId == profileTypeId.Value || x.ProfileTypeId == Guid.Empty);
            }
            types = all.OrderBy(x => x.Name).ToList();
        }
        else
        {
            types = (await _repository.GetActiveByProfileTypeAsync(profileTypeId, cancellationToken)).ToList();
        }

        var typeIds = types.Select(t => t.Id).ToList();
        var translations = await _translationRepo.GetByOwnerIdsAsync(typeIds, cancellationToken);
        var preferredLang = _currentUserService.DataLanguage;

        var dtos = types.Select(t =>
        {
            var trans = translations.GetValueOrDefault(t.Id);
            var translatedName = Shared.Application.Translations.TranslationResolver.Resolve(
                t.Name, nameof(DocumentType.Name), trans, preferredLang);
            var translatedDesc = Shared.Application.Translations.TranslationResolver.Resolve(
                t.Description ?? string.Empty, nameof(DocumentType.Description), trans, preferredLang);

            return new DocumentTypeDto(
                t.Id,
                translatedName,
                t.Code,
                string.IsNullOrEmpty(translatedDesc) ? null : translatedDesc,
                t.IsRenewable,
                t.RenewalAlertDays,
                t.IsRequired,
                t.IsActive,
                t.ProfileTypeId);
        });

        return Ok(dtos);
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
            request.ProfileTypeId,
            request.Name,
            request.Code,
            request.Description,
            request.IsRenewable,
            request.RenewalAlertDays,
            request.IsRequired);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        await _repository.AddAsync(result.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Document type deactivated." });
    }

    [Permission(
        resourceKey: "document.type",
        action: "activate",
        description: "Activate a document type (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var docType = await _repository.GetByIdAsync(id, cancellationToken);
        if (docType is null)
            return NotFound(new { error = "Document type not found." });

        docType.Activate();
        await _repository.UpdateAsync(docType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Document type activated." });
    }

    [Permission(
        resourceKey: "document.type",
        action: "read",
        description: "Get translations for a document type",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("{id:guid}/translations")]
    public async Task<IActionResult> GetTranslations(Guid id, CancellationToken cancellationToken)
    {
        var translations = await _translationRepo.GetByOwnerIdAsync(id, cancellationToken);
        var result = translations.ToDictionary(
            t => t.CultureCode,
            t => t.Fields
        );
        return Ok(result);
    }

    [Permission(
        resourceKey: "document.type",
        action: "update",
        description: "Update translations for a document type",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPut("{id:guid}/translations/{cultureCode}")]
    public async Task<IActionResult> UpdateTranslation(
        Guid id,
        string cultureCode,
        [FromBody] Dictionary<string, string> fields,
        CancellationToken cancellationToken)
    {
        await _translationRepo.UpsertAsync(id, cultureCode, fields, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}