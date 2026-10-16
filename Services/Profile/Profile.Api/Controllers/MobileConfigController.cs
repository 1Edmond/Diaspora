using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Profile.Application.DTOs;
using Shared.Messaging.Constants;
using System;
using System.Collections.Generic;

namespace Profile.Api.Controllers;

[ApiController]
[Route("api/config/mobile")]
public class MobileConfigController(
    IConfiguration _configuration,
    ILogger<MobileConfigController> _logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetMobileConfig()
    {
        _logger.LogInformation("[MobileConfig] Serving bootstrap configuration");

        var activeModules = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            ["wallet"] = _configuration.GetValue("MobileConfig:ActiveModules:wallet", true),
            ["documents"] = _configuration.GetValue("MobileConfig:ActiveModules:documents", true),
            ["marketplace"] = _configuration.GetValue("MobileConfig:ActiveModules:marketplace", true),
            ["procedures"] = _configuration.GetValue("MobileConfig:ActiveModules:procedures", true),
            ["chat"] = _configuration.GetValue("MobileConfig:ActiveModules:chat", true),
            ["community"] = _configuration.GetValue("MobileConfig:ActiveModules:community", false),
            ["committee"] = _configuration.GetValue("MobileConfig:ActiveModules:committee", false),
            ["freelance"] = _configuration.GetValue("MobileConfig:ActiveModules:freelance", false),
            ["services"] = _configuration.GetValue("MobileConfig:ActiveModules:services", true),
            ["settings"] = _configuration.GetValue("MobileConfig:ActiveModules:settings", true),
            ["profile"] = _configuration.GetValue("MobileConfig:ActiveModules:profile", true)
        };

        var config = new MobileBootstrapConfigDto
        {
            ActiveModules = activeModules,
            ParcelTransportCategoryId = ServiceDataConstants.ParcelTransportCategoryId,
            DefaultLocale = _configuration.GetValue("MobileConfig:DefaultLocale", "fr") ?? "fr",
            SupportedLocales = _configuration.GetSection("MobileConfig:SupportedLocales").Get<string[]>() ?? ["fr", "en", "ru"],
            MinAppVersion = _configuration.GetValue("MobileConfig:MinAppVersion", "1.0.0") ?? "1.0.0"
        };

        return Ok(config);
    }
}
