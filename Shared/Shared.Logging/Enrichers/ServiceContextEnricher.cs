using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Logging.Enrichers;

public class ServiceContextEnricher : ILogEventEnricher
{
    private readonly string _serviceName;
    private readonly string _serviceVersion;
    private readonly string _environment;

    public ServiceContextEnricher(string serviceName, string serviceVersion, string environment)
    {
        _serviceName = serviceName;
        _serviceVersion = serviceVersion;
        _environment = environment;
    }   

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ServiceName", _serviceName));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ServiceVersion", _serviceVersion));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Environment", _environment));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MachineName", System.Environment.MachineName));
    }
}