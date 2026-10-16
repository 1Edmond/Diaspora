using System;
using System.Collections.Generic;

namespace Profile.Application.DTOs;

public class MobileBootstrapConfigDto
{
    public Dictionary<string, bool> ActiveModules { get; set; } = new();
    public Guid ParcelTransportCategoryId { get; set; }
    public string DefaultLocale { get; set; } = "fr";
    public string[] SupportedLocales { get; set; } = ["fr", "en", "ru"];
    public string MinAppVersion { get; set; } = "1.0.0";
}
