using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Constants;

public static class QueuesConstants
{
    public const string DefaultRabbitMqQueue = "blood-donation-event";
    public const string IdentityServiceQueue = "identity-service-queue";
    public const string ProfileServiceQueue = "profile.service.queue";
    public const string DocumentServiceQueue = "document.service.queue";
    public const string ProcedureServiceQueue = "procedure.service.queue";
    
}
