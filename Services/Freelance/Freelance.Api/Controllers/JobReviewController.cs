using Freelance.Application.Commands;
using Freelance.Application.Dtos;
using Freelance.Application.Interfaces;
using Freelance.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/freelance/job-reviews")]
public class JobReviewController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService) : ControllerBase
{
    [Permission(
        resourceKey: "freelance.job-review",
        action: "read",
        description: "View job reviews for an application",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("application/{jobApplicationId:guid}")]
    public async Task<IActionResult> GetByApplication(Guid jobApplicationId)
    {
        // For now return all reviews for the application
        // In a real scenario, you'd add a specific query for this
        return Ok(new { message = "Use GetReputation endpoint for aggregated reviews" });
    }

    [Permission(
        resourceKey: "freelance.job-review",
        action: "read-admin",
        description: "View all freelance job reviews for moderation",
        recommendedRoles: new[] { "Admin", "SuperAdmin", "Moderator" }
    )]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? rating = null,
        [FromQuery] string? search = null)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        var result = await _messageBus.InvokeAsync<Result<PagedList<JobReviewDto>>>(
            new GetJobReviewsAdminQuery(page, pageSize, rating, search));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "freelance.job-review",
        action: "delete",
        description: "Delete an abusive freelance review",
        recommendedRoles: new[] { "Admin", "SuperAdmin", "Moderator" }
    )]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] IJobReviewRepository reviewRepo,
        [FromServices] Shared.Domain.Interfaces.IUnitOfWork uow)
    {
        var review = await reviewRepo.GetByIdAsync(id);
        if (review == null) return NotFound(new { error = "Review not found." });

        await reviewRepo.DeleteAsync(review);
        await uow.SaveChangesAsync();
        return NoContent();
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}