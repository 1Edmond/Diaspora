using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Commons;
using Wolverine;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/freelance/job-categories")]
public class JobCategoryController(
    IMessageBus _messageBus) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var result = await _messageBus.InvokeAsync<Result<List<JobCategoryDto>>>(
            new GetJobCategoriesQuery(activeOnly));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }
}