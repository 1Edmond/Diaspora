using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events;

public interface IIntegrationEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent);
}
