using Microsoft.AspNetCore.Mvc;
using Profile.Application.Extensions;
using Profile.Application.Interfaces;
using Profile.Application.Requests;
using Profile.Domain.Entities;
using Shared.Security.Attributes;

namespace Profile.Api.Controllers;

[ApiController]
[Route("api/profile-types")]
public class ProfileTypeController(
    IProfileTypeRepository _repository,
    ILogger<ProfileTypeController> _logger) : ControllerBase
{
    [Permission(
        resourceKey: "profile.type",
        action: "list",
        description: "List all active profile types",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var types = await _repository.GetAllActiveAsync(cancellationToken);
        return Ok(types.Select(t => t.ToDto()));
    }

    [Permission(
        resourceKey: "profile.type",
        action: "create",
        description: "Create a new profile type (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProfileTypeRequest request,
        CancellationToken cancellationToken)
    {
        var exists = await _repository.GetByCountryCodeAsync(request.CountryCode, cancellationToken);
        if (exists is not null)
            return Conflict(new { error = $"Profile type with country code '{request.CountryCode}' already exists." });

        var profileType = ProfileType.Create(
            Guid.NewGuid(),
            request.CountryName,
            request.CountryCode,
            request.PhoneIndicatif,
            request.Currency);

        await _repository.AddAsync(profileType, cancellationToken);

        return Ok(profileType.ToDto());
    }

    [Permission(
        resourceKey: "profile.type",
        action: "update",
        description: "Update a profile type (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreateProfileTypeRequest request,
        CancellationToken cancellationToken)
    {
        var profileType = await _repository.GetByIdAsync(id, cancellationToken);
        if (profileType is null)
            return NotFound(new { error = "Profile type not found." });

        var existing = await _repository.GetByCountryCodeAsync(request.CountryCode, cancellationToken);
        if (existing is not null && existing.Id != id)
            return Conflict(new { error = $"Profile type with country code '{request.CountryCode}' already exists." });

        profileType.Update(
            request.CountryName,
            request.CountryCode,
            request.PhoneIndicatif,
            request.Currency);

        await _repository.UpdateAsync(profileType, cancellationToken);

        return Ok(profileType.ToDto());
    }

    [Permission(
        resourceKey: "profile.type",
        action: "deactivate",
        description: "Deactivate a profile type (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var profileType = await _repository.GetByIdAsync(id, cancellationToken);
        if (profileType is null)
            return NotFound(new { error = "Profile type not found." });

        profileType.Deactivate();
        await _repository.UpdateAsync(profileType, cancellationToken);

        return Ok(new { message = "Profile type deactivated." });
    }

    [Permission(
        resourceKey: "profile.type",
        action: "activate",
        description: "Activate a profile type (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var profileType = await _repository.GetByIdAsync(id, cancellationToken);
        if (profileType is null)
            return NotFound(new { error = "Profile type not found." });

        profileType.Activate();
        await _repository.UpdateAsync(profileType, cancellationToken);

        return Ok(new { message = "Profile type activated." });
    }
}
