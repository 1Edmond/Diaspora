using System;
using System.Collections.Generic;

namespace Profile.Application.DTOs;

public class MobileBootstrapConfigDto
{
    public Guid? ProfileTypeId { get; set; }
    public string? CountryCode { get; set; }
    public Dictionary<string, bool> ActiveModules { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Guid ParcelTransportCategoryId { get; set; }
    public string DefaultLocale { get; set; } = "fr";
    public string[] SupportedLocales { get; set; } = ["fr", "en", "ru"];
    public string MinAppVersion { get; set; } = "1.0.0";
    public bool ForceUpdate { get; set; }
    public bool IsMaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class UpsertMobileConfigRequest
{
    public Dictionary<string, bool>? ActiveModules { get; set; }
    public string? MinAppVersion { get; set; }
    public bool ForceUpdate { get; set; }
    public bool IsMaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public string? DefaultLocale { get; set; }
    public List<string>? SupportedLocales { get; set; }
}
