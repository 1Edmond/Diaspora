using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;

namespace Profile.Domain.Entities;

public sealed class ProfileTypeMobileConfig : Entity
{
    public Guid? ProfileTypeId { get; private set; } // null = Global default
    public string? CountryCode { get; private set; }
    public Dictionary<string, bool> ActiveModules { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
    public string MinAppVersion { get; private set; } = "1.0.0";
    public bool ForceUpdate { get; private set; }
    public bool IsMaintenanceMode { get; private set; }
    public string? MaintenanceMessage { get; private set; }
    public string DefaultLocale { get; private set; } = "fr";
    public List<string> SupportedLocales { get; private set; } = new() { "fr", "en", "ru" };

    private ProfileTypeMobileConfig() { }

    public ProfileTypeMobileConfig(
        Guid id,
        Guid? profileTypeId,
        string? countryCode,
        Dictionary<string, bool> activeModules,
        string minAppVersion = "1.0.0",
        bool forceUpdate = false,
        bool isMaintenanceMode = false,
        string? maintenanceMessage = null,
        string defaultLocale = "fr",
        List<string>? supportedLocales = null,
        string? updatedBy = null) : base(id)
    {
        Id = id;
        ProfileTypeId = profileTypeId;
        CountryCode = countryCode?.ToUpperInvariant();
        ActiveModules = activeModules ?? new(StringComparer.OrdinalIgnoreCase);
        MinAppVersion = string.IsNullOrWhiteSpace(minAppVersion) ? "1.0.0" : minAppVersion.Trim();
        ForceUpdate = forceUpdate;
        IsMaintenanceMode = isMaintenanceMode;
        MaintenanceMessage = maintenanceMessage;
        DefaultLocale = string.IsNullOrWhiteSpace(defaultLocale) ? "fr" : defaultLocale.Trim();
        SupportedLocales = supportedLocales ?? new() { "fr", "en", "ru" };
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Update(
        Dictionary<string, bool> activeModules,
        string minAppVersion,
        bool forceUpdate,
        bool isMaintenanceMode,
        string? maintenanceMessage,
        string defaultLocale,
        List<string>? supportedLocales,
        string? updatedBy = null)
    {
        ActiveModules = activeModules ?? new(StringComparer.OrdinalIgnoreCase);
        MinAppVersion = string.IsNullOrWhiteSpace(minAppVersion) ? "1.0.0" : minAppVersion.Trim();
        ForceUpdate = forceUpdate;
        IsMaintenanceMode = isMaintenanceMode;
        MaintenanceMessage = maintenanceMessage;
        DefaultLocale = string.IsNullOrWhiteSpace(defaultLocale) ? "fr" : defaultLocale.Trim();
        if (supportedLocales != null && supportedLocales.Count > 0)
        {
            SupportedLocales = supportedLocales;
        }
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}
