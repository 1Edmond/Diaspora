using Marketplace.Application.Commands;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Marketplace.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolverine;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/marketplace/payment-policies")]
public class PaymentPolicyController(
    IMessageBus _messageBus,
    ILogger<PaymentPolicyController> _logger) : ControllerBase
{
    [Permission("PaymentPolicies.View", "Consulter les politiques de règlement")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _messageBus.InvokeAsync<Result<List<ProfileTypePaymentPolicyDto>>>(
            new GetPaymentPoliciesQuery());

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission("PaymentPolicies.View", "Consulter les politiques de règlement")]
    [HttpGet("by-profile-type/{profileTypeId:guid}")]
    public async Task<IActionResult> GetByProfileType(Guid profileTypeId)
    {
        var result = await _messageBus.InvokeAsync<Result<ProfileTypePaymentPolicyDto>>(
            new GetPaymentPolicyByProfileTypeQuery(profileTypeId));

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpGet("by-country/{countryCode}")]
    public async Task<IActionResult> GetByCountry(string countryCode)
    {
        var result = await _messageBus.InvokeAsync<Result<ProfileTypePaymentPolicyDto>>(
            new ResolvePaymentPolicyQuery(null, countryCode));

        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpGet("resolve")]
    public async Task<IActionResult> Resolve([FromQuery] Guid? profileTypeId, [FromQuery] string? countryCode)
    {
        var result = await _messageBus.InvokeAsync<Result<ProfileTypePaymentPolicyDto>>(
            new ResolvePaymentPolicyQuery(profileTypeId, countryCode));

        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [Permission("PaymentPolicies.Create", "Créer une politique de règlement")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentPolicyCommand command, CancellationToken ct)
    {
        var result = await _messageBus.InvokeAsync<Result<Guid>>(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetByProfileType), new { profileTypeId = command.ProfileTypeId }, new { id = result.Value });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [Permission("PaymentPolicies.Update", "Modifier une politique de règlement")]
    [HttpPut("{profileTypeId:guid}")]
    public async Task<IActionResult> Update(Guid profileTypeId, [FromBody] UpdateProfileTypePaymentPolicyRequest request, CancellationToken ct = default)
    {
        var command = new UpdatePaymentPolicyCommand(
            profileTypeId,
            request.CountryCode,
            request.AllowedPolicy,
            request.DefaultCashTiming,
            request.DirectBookingFeeEnabled,
            request.DirectBookingFeeFixedAmount,
            request.DirectBookingFeeRate,
            request.Currency,
            request.IsActive
        );

        var result = await _messageBus.InvokeAsync<Result>(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }
}
