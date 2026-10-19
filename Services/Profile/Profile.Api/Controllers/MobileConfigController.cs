using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Profile.Application.DTOs;
using Profile.Application.Interfaces;
using Profile.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Messaging.Constants;
using Shared.Security.Attributes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Profile.Api.Controllers;

[ApiController]
[Route("api/config/mobile")]
public class MobileConfigController(
    IMobileConfigRepository _configRepository,
    IProfileTypeRepository _profileTypeRepository,
    IProfileRepository _profileRepository,
    IConfiguration _configuration,
    ICurrentUserService _currentUserService,
    ILogger<MobileConfigController> _logger) : ControllerBase
{
    private static readonly Dictionary<string, bool> DefaultActiveModules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["wallet"] = true,
        ["documents"] = true,
        ["marketplace"] = true,
        ["procedures"] = true,
        ["chat"] = true,
        ["community"] = false,
        ["committee"] = false,
        ["freelance"] = false,
        ["services"] = true,
        ["settings"] = true,
        ["profile"] = true
    };

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetMobileConfig(
        [FromQuery] Guid? profileTypeId = null,
        [FromQuery] string? countryCode = null,
        CancellationToken cancellationToken = default)
    {
        if ((!profileTypeId.HasValue || profileTypeId.Value == Guid.Empty) && !string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            if (Guid.TryParse(_currentUserService.UserId, out var userId))
            {
                var profiles = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
                var userProfile = profiles?.FirstOrDefault(p => p.Status == Domain.Enums.ProfileStatus.Activate || p.Status == Domain.Enums.ProfileStatus.Verified)
                    ?? profiles?.FirstOrDefault();

                if (userProfile != null && userProfile.ProfileTypeId != Guid.Empty)
                {
                    profileTypeId = userProfile.ProfileTypeId;
                    if (string.IsNullOrWhiteSpace(countryCode) && userProfile.ProfileType != null)
                    {
                        countryCode = userProfile.ProfileType.CountryCode;
                    }
                }
            }
        }

        _logger.LogInformation("[MobileConfig] Serving bootstrap config (profileTypeId={ProfileTypeId}, countryCode={CountryCode})",
            profileTypeId, countryCode);

        ProfileTypeMobileConfig? config = null;

        if (profileTypeId.HasValue && profileTypeId.Value != Guid.Empty)
        {
            config = await _configRepository.GetByProfileTypeIdAsync(profileTypeId.Value, cancellationToken);
        }

        if (config == null && !string.IsNullOrWhiteSpace(countryCode))
        {
            config = await _configRepository.GetByCountryCodeAsync(countryCode.Trim(), cancellationToken);
        }

        if (config == null)
        {
            config = await _configRepository.GetGlobalAsync(cancellationToken);
        }

        if (config != null)
        {
            return Ok(new MobileBootstrapConfigDto
            {
                ProfileTypeId = config.ProfileTypeId,
                CountryCode = config.CountryCode,
                ActiveModules = config.ActiveModules,
                ParcelTransportCategoryId = ServiceDataConstants.ParcelTransportCategoryId,
                DefaultLocale = config.DefaultLocale,
                SupportedLocales = config.SupportedLocales.ToArray(),
                MinAppVersion = config.MinAppVersion,
                ForceUpdate = config.ForceUpdate,
                IsMaintenanceMode = config.IsMaintenanceMode,
                MaintenanceMessage = config.MaintenanceMessage,
                UpdatedAt = config.UpdatedAt,
                UpdatedBy = config.UpdatedBy
            });
        }

        // Fallback vers IConfiguration si aucune ligne en base
        var fallbackModules = new Dictionary<string, bool>(DefaultActiveModules, StringComparer.OrdinalIgnoreCase);
        foreach (var key in DefaultActiveModules.Keys)
        {
            fallbackModules[key] = _configuration.GetValue($"MobileConfig:ActiveModules:{key}", DefaultActiveModules[key]);
        }

        return Ok(new MobileBootstrapConfigDto
        {
            ActiveModules = fallbackModules,
            ParcelTransportCategoryId = ServiceDataConstants.ParcelTransportCategoryId,
            DefaultLocale = _configuration.GetValue("MobileConfig:DefaultLocale", "fr") ?? "fr",
            SupportedLocales = _configuration.GetSection("MobileConfig:SupportedLocales").Get<string[]>() ?? ["fr", "en", "ru"],
            MinAppVersion = _configuration.GetValue("MobileConfig:MinAppVersion", "1.0.0") ?? "1.0.0",
            ForceUpdate = false,
            IsMaintenanceMode = false
        });
    }

    [Permission(
        resourceKey: "profile.mobile-config",
        action: "read",
        description: "View all mobile configurations",
        recommendedRoles: new[] { "Admin", "SuperAdmin" }
    )]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllAdmin(CancellationToken cancellationToken = default)
    {
        var configs = await _configRepository.GetAllAsync(cancellationToken);
        return Ok(configs);
    }

    [Permission(
        resourceKey: "profile.mobile-config",
        action: "read",
        description: "View mobile configuration for a specific profile type",
        recommendedRoles: new[] { "Admin", "SuperAdmin" }
    )]
    [HttpGet("by-profile-type/{profileTypeId:guid}")]
    public async Task<IActionResult> GetByProfileType(Guid profileTypeId, CancellationToken cancellationToken = default)
    {
        var config = await _configRepository.GetByProfileTypeIdAsync(profileTypeId, cancellationToken);
        if (config == null) return NotFound(new { message = "No specific configuration found for this profile type." });
        return Ok(config);
    }

    [Permission(
        resourceKey: "profile.mobile-config",
        action: "manage",
        description: "Update global mobile configuration",
        recommendedRoles: new[] { "Admin", "SuperAdmin" }
    )]
    [HttpPut("global")]
    public async Task<IActionResult> UpsertGlobal(
        [FromBody] UpsertMobileConfigRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentAdmin = _currentUserService.Email ?? _currentUserService.UserId ?? "Admin";
        var existing = await _configRepository.GetGlobalAsync(cancellationToken);

        var modules = request.ActiveModules ?? new Dictionary<string, bool>(DefaultActiveModules, StringComparer.OrdinalIgnoreCase);

        if (existing == null)
        {
            var newConfig = new ProfileTypeMobileConfig(
                id: Guid.NewGuid(),
                profileTypeId: null,
                countryCode: null,
                activeModules: modules,
                minAppVersion: request.MinAppVersion ?? "1.0.0",
                forceUpdate: request.ForceUpdate,
                isMaintenanceMode: request.IsMaintenanceMode,
                maintenanceMessage: request.MaintenanceMessage,
                defaultLocale: request.DefaultLocale ?? "fr",
                supportedLocales: request.SupportedLocales,
                updatedBy: currentAdmin
            );
            await _configRepository.AddAsync(newConfig, cancellationToken);
            return Ok(newConfig);
        }

        existing.Update(
            activeModules: modules,
            minAppVersion: request.MinAppVersion ?? existing.MinAppVersion,
            forceUpdate: request.ForceUpdate,
            isMaintenanceMode: request.IsMaintenanceMode,
            maintenanceMessage: request.MaintenanceMessage,
            defaultLocale: request.DefaultLocale ?? existing.DefaultLocale,
            supportedLocales: request.SupportedLocales,
            updatedBy: currentAdmin
        );
        await _configRepository.UpdateAsync(existing, cancellationToken);
        return Ok(existing);
    }

    [Permission(
        resourceKey: "profile.mobile-config",
        action: "manage",
        description: "Update mobile configuration for profile type",
        recommendedRoles: new[] { "Admin", "SuperAdmin" }
    )]
    [HttpPut("by-profile-type/{profileTypeId:guid}")]
    public async Task<IActionResult> UpsertByProfileType(
        Guid profileTypeId,
        [FromBody] UpsertMobileConfigRequest request,
        CancellationToken cancellationToken = default)
    {
        var profileType = await _profileTypeRepository.GetByIdAsync(profileTypeId, cancellationToken);
        if (profileType == null) return NotFound(new { error = "ProfileType not found." });

        var currentAdmin = _currentUserService.Email ?? _currentUserService.UserId ?? "Admin";
        var existing = await _configRepository.GetByProfileTypeIdAsync(profileTypeId, cancellationToken);

        var modules = request.ActiveModules ?? new Dictionary<string, bool>(DefaultActiveModules, StringComparer.OrdinalIgnoreCase);

        if (existing == null)
        {
            var newConfig = new ProfileTypeMobileConfig(
                id: Guid.NewGuid(),
                profileTypeId: profileTypeId,
                countryCode: profileType.CountryCode,
                activeModules: modules,
                minAppVersion: request.MinAppVersion ?? "1.0.0",
                forceUpdate: request.ForceUpdate,
                isMaintenanceMode: request.IsMaintenanceMode,
                maintenanceMessage: request.MaintenanceMessage,
                defaultLocale: request.DefaultLocale ?? "fr",
                supportedLocales: request.SupportedLocales,
                updatedBy: currentAdmin
            );
            await _configRepository.AddAsync(newConfig, cancellationToken);
            return Ok(newConfig);
        }

        existing.Update(
            activeModules: modules,
            minAppVersion: request.MinAppVersion ?? existing.MinAppVersion,
            forceUpdate: request.ForceUpdate,
            isMaintenanceMode: request.IsMaintenanceMode,
            maintenanceMessage: request.MaintenanceMessage,
            defaultLocale: request.DefaultLocale ?? existing.DefaultLocale,
            supportedLocales: request.SupportedLocales,
            updatedBy: currentAdmin
        );
        await _configRepository.UpdateAsync(existing, cancellationToken);
        return Ok(existing);
    }

    [Permission(
        resourceKey: "profile.mobile-config",
        action: "manage",
        description: "Delete mobile configuration override for profile type",
        recommendedRoles: new[] { "Admin", "SuperAdmin" }
    )]
    [HttpDelete("by-profile-type/{profileTypeId:guid}")]
    public async Task<IActionResult> DeleteByProfileType(
        Guid profileTypeId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _configRepository.GetByProfileTypeIdAsync(profileTypeId, cancellationToken);
        if (existing != null)
        {
            await _configRepository.DeleteAsync(existing, cancellationToken);
        }
        return NoContent();
    }
}
