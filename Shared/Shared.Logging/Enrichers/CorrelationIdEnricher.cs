using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;


namespace Shared.Logging.Enrichers;

public class CorrelationIdEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
            }
            else
            {
                var newCorrelationId = Guid.NewGuid().ToString();
                httpContext.Items["CorrelationId"] = newCorrelationId;
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", newCorrelationId));
            }
        }
    }
}