
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Logging.Configurations;

public class ServiceLoggingConfiguration
{
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceVersion { get; set; } = "1.0.0";
    public string Environment { get; set; } = "Development";
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnableFileLogging { get; set; } = false;
    public string? LogFilePath { get; set; }
}