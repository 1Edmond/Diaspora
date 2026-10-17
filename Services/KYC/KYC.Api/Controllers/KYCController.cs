using KYC.Application.Commands;
using KYC.Application.Dtos;
using KYC.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace KYC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KYCController(
    IMessageBus messageBus,
    ILogger<KYCController> logger,
    ICurrentUserService currentUserService,
    IFileStorageService _fileStorage) : ControllerBase
{
    [Permission(
        resourceKey: "kyc.verifications",
        action: "create",
        description: "Create a new KYC verification",
        recommendedRoles: new[] { "User", "Moderator" }
    )]
    [HttpPost]
    public async Task<IActionResult> CreateVerification([FromBody] CreateKYCVerificationCommand command)
    {
        var userId = GetCurrentUserId();
        command = command with { UserId = userId };

        var result = await messageBus.InvokeAsync<Result<Guid>>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(
            nameof(GetVerification),
            new { id = result.Value },
            new { id = result.Value });
    }

    [Permission(
        resourceKey: "kyc.verifications",
        action: "read",
        description: "Get a KYC verification by ID",
        recommendedRoles: new[] { "Admin", "Moderator", "User" }
    )]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetVerification(Guid id)
    {
        var query = new GetKYCVerificationQuery(id);
        var result = await messageBus.InvokeAsync<Result<KYCVerificationDto>>(query);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || User.IsInRole("Moderator");

        if (result.Value.UserId != userId && !isAdmin)
            return Forbid();

        return Ok(DataResponse<KYCVerificationDto>.SuccessResponse(result.Value));
    }

    [Permission(
        resourceKey: "kyc.verifications",
        action: "read-self",
        description: "Get the current user's own KYC verification",
        recommendedRoles: new[] { "User", "Moderator" }
    )]
    [HttpGet("my-verification")]
    public async Task<IActionResult> GetMyVerification()
    {
        var userId = GetCurrentUserId();
        var query = new GetKYCVerificationByUserQuery(userId);
        var result = await messageBus.InvokeAsync<Result<KYCVerificationDto>>(query);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(DataResponse<KYCVerificationDto>.SuccessResponse(result.Value));
    }

    [Permission(
        resourceKey: "kyc.verifications",
        action: "read",
        description: "Get KYC verification by user ID",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetVerificationByUserId(Guid userId)
    {
        var query = new GetKYCVerificationByUserQuery(userId);
        var result = await messageBus.InvokeAsync<Result<KYCVerificationDto>>(query);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(DataResponse<KYCVerificationDto>.SuccessResponse(result.Value));
    }

    [Permission(
        resourceKey: "kyc.verifications",
        action: "read",
        description: "Get KYC compliance status for a user",
        recommendedRoles: new[] { "Admin", "Moderator", "User" }
    )]
    [HttpGet("user/{userId:guid}/compliance")]
    public async Task<IActionResult> GetUserCompliance(Guid userId, [FromQuery] Guid? profileTypeId = null)
    {
        var isStaff = User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || User.IsInRole("Moderator");
        if (!isStaff && userId != GetCurrentUserId())
            return Forbid();

        var query = new GetKycComplianceQuery(userId, profileTypeId);
        var result = await messageBus.InvokeAsync<Result<KycComplianceDto>>(query);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(DataResponse<KycComplianceDto>.SuccessResponse(result.Value));
    }

    [Permission(
        resourceKey: "kyc.verifications",
        action: "list",
        description: "Get paginated list of all KYC verifications",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpGet]
    public async Task<IActionResult> GetVerifications([FromQuery] GetKYCVerificationsQuery query)
    {
        var result = await messageBus.InvokeAsync<Result<PagedList<KYCVerificationDto>>>(query);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "kyc.verifications",
        action: "list-pending",
        description: "Get paginated list of pending KYC verifications",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingVerifications([FromQuery] GetPendingKYCVerificationsQuery query)
    {
        var result = await messageBus.InvokeAsync<Result<PagedList<KYCVerificationDto>>>(query);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "kyc.documents",
        action: "upload",
        description: "Upload a document for a KYC verification",
        recommendedRoles: new[] { "User", "Moderator" }
    )]
    [HttpPost("{id:guid}/documents")]
    public async Task<IActionResult> UploadDocument(Guid id, [FromForm] IFormFile file, [FromForm] string documentType)
    {
        var verification = await messageBus.InvokeAsync<Result<KYCVerificationDto>>(
            new GetKYCVerificationQuery(id));

        if (verification.IsFailure)
            return NotFound(new { error = verification.Error });

        var userId = GetCurrentUserId();
        if (verification.Value.UserId != userId)
            return Forbid();

        var command = new UploadKYCDocumentCommand(id, documentType, file);
        var result = await messageBus.InvokeAsync<Result<Guid>>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { documentId = result.Value });
    }

    [Permission(
        resourceKey: "kyc.documents",
        action: "delete",
        description: "Remove a document from a KYC verification",
        recommendedRoles: new[] { "User", "Moderator" }
    )]
    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    public async Task<IActionResult> RemoveDocument(Guid id, Guid documentId)
    {
        var verification = await messageBus.InvokeAsync<Result<KYCVerificationDto>>(
            new GetKYCVerificationQuery(id));

        if (verification.IsFailure)
            return NotFound(new { error = verification.Error });

        var userId = GetCurrentUserId();
        if (verification.Value.UserId != userId)
            return Forbid();

        var command = new RemoveKYCDocumentCommand(id, documentId);
        var result = await messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(BaseResponse.SuccessResponse("Document removed successfully"));
    }

    [Permission(
        resourceKey: "kyc.verifications",
        action: "submit",
        description: "Submit a KYC verification for review",
        recommendedRoles: new[] { "User", "Moderator" }
    )]
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> SubmitVerification(Guid id)
    {
        var verification = await messageBus.InvokeAsync<Result<KYCVerificationDto>>(
            new GetKYCVerificationQuery(id));

        if (verification.IsFailure)
            return NotFound(new { error = verification.Error });

        var userId = GetCurrentUserId();
        if (verification.Value.UserId != userId)
            return Forbid();

        var command = new SubmitKYCVerificationCommand(id);
        var result = await messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(BaseResponse.SuccessResponse("KYC verification submitted successfully"));
    }

    [Permission(
        resourceKey: "kyc.verifications",
        action: "approve",
        description: "Approve a KYC verification",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveVerification(Guid id, [FromBody] ApproveKYCCommand command)
    {
        var reviewerId = GetCurrentUserId();
        var level = string.IsNullOrWhiteSpace(command.Level) ? Shared.Domain.Enums.KYCLevel.Intermediate.ToString() : command.Level;
        command = command with { VerificationId = id, ReviewerId = reviewerId, Level = level };

        var result = await messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(BaseResponse.SuccessResponse("KYC verification approved successfully"));
    }

    [Permission(
        resourceKey: "kyc.verifications",
        action: "reject",
        description: "Reject a KYC verification",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectVerification(Guid id, [FromBody] RejectKYCCommand command)
    {
        var reviewerId = GetCurrentUserId();
        command = command with { VerificationId = id, ReviewerId = reviewerId };

        var result = await messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(BaseResponse.SuccessResponse("KYC verification rejected"));
    }

    [Permission(
        resourceKey: "kyc.verifications",
        action: "request-info",
        description: "Request additional information from a user for their KYC verification",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpPost("{id:guid}/request-info")]
    public async Task<IActionResult> RequestMoreInfo(Guid id, [FromBody] RequestMoreInfoCommand command)
    {
        var reviewerId = GetCurrentUserId();
        command = command with { VerificationId = id, ReviewerId = reviewerId };

        var result = await messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(BaseResponse.SuccessResponse("More information requested"));
    }

    [Permission(
        resourceKey: "kyc.documents",
        action: "download",
        description: "Download a decrypted KYC document",
        recommendedRoles: new[] { "Admin", "Moderator", "User" }
    )]
    [HttpGet("{id:guid}/documents/{documentId:guid}/download")]
    public async Task<IActionResult> DownloadDocument(Guid id, Guid documentId)
    {
        var verification = await messageBus.InvokeAsync<Result<KYCVerificationDto>>(
            new GetKYCVerificationQuery(id));

        if (verification.IsFailure)
            return NotFound(new { error = verification.Error });

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || User.IsInRole("Moderator");

        if (verification.Value.UserId != userId && !isAdmin)
            return Forbid();

        var document = verification.Value.Documents.FirstOrDefault(d => d.Id == documentId);
        if (document == null)
            return NotFound(new { error = "Document not found" });

        var fileResult = await _fileStorage.DownloadDecryptedFileAsync(
            document.FilePath,
            id.ToString(),
            "KYCDocument");

        if (fileResult.IsFailure)
            return BadRequest(new { error = fileResult.Error });

        var stream = fileResult.Value;

        var contentType = document.FileName.EndsWith(".pdf")
            ? "application/pdf"
            : "image/jpeg";

        return File(stream, contentType, document.FileName);
    }

    [Permission(
        resourceKey: "kyc.verifications",
        action: "start-review",
        description: "Start the review process for a KYC verification",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpPost("{id:guid}/start-review")]
    public async Task<IActionResult> StartReview(Guid id)
    {
        var reviewerId = GetCurrentUserId();
        var command = new StartKYCReviewCommand(id, reviewerId);

        var result = await messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(BaseResponse.SuccessResponse("Review started"));
    }

    [Permission(
        resourceKey: "kyc.requirements",
        action: "read",
        description: "Get country KYC requirements",
        recommendedRoles: new[] { "Admin", "Moderator", "User" }
    )]
    [HttpGet("requirements")]
    public async Task<IActionResult> GetRequirements([FromQuery] Guid? profileTypeId)
    {
        var query = new GetCountryKycRequirementsQuery(profileTypeId);
        var result = await messageBus.InvokeAsync<Result<List<CountryKycRequirementDto>>>(query);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "kyc.requirements",
        action: "read",
        description: "Get country KYC requirements by profile type",
        recommendedRoles: new[] { "Admin", "Moderator", "User" }
    )]
    [HttpGet("requirements/{profileTypeId:guid}")]
    public async Task<IActionResult> GetRequirementsByProfileTypeId(Guid profileTypeId)
    {
        var query = new GetCountryKycRequirementsQuery(profileTypeId);
        var result = await messageBus.InvokeAsync<Result<List<CountryKycRequirementDto>>>(query);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "kyc.requirements",
        action: "manage",
        description: "Create country KYC requirement",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("requirements")]
    public async Task<IActionResult> CreateRequirement([FromBody] CreateCountryKycRequirementCommand command)
    {
        var result = await messageBus.InvokeAsync<Result<CountryKycRequirementDto>>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(
            nameof(GetRequirementsByProfileTypeId),
            new { profileTypeId = result.Value.ProfileTypeId },
            result.Value);
    }

    [Permission(
        resourceKey: "kyc.requirements",
        action: "manage",
        description: "Update country KYC requirement",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPut("requirements/{id:guid}")]
    public async Task<IActionResult> UpdateRequirement(Guid id, [FromBody] UpdateCountryKycRequirementCommand command)
    {
        command = command with { Id = id };
        var result = await messageBus.InvokeAsync<Result<CountryKycRequirementDto>>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "kyc.requirements",
        action: "manage",
        description: "Delete country KYC requirement",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpDelete("requirements/{id:guid}")]
    public async Task<IActionResult> DeleteRequirement(Guid id)
    {
        var command = new DeleteCountryKycRequirementCommand(id);
        var result = await messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(currentUserService.UserId!);
    }
}